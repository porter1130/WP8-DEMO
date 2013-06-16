using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnderControl.Communication.Helpers;
using UnderControl.Shared;
using UnderControl.Shared.Sockets;

#if WINDOWS_PHONE
using Microsoft.Phone.Net.NetworkInformation;
#endif

namespace UnderControl.Communication.Sockets
{
    /// <summary>
    /// An implementation of the <c>IUdpSocketWrapper</c> interface.
    /// </summary>
    internal partial class UdpSocketWrapper : IUdpSocketWrapper
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int ReceiveBufferSize = 1024;

        private SocketAsyncEventArgs _socketOperation;
        private Socket _currentSocket;

        /// <summary>
        /// Occurs when data was sent successfully.
        /// Due to the nature of UDP, this does not mean that the data was successfully received on the other end.
        /// </summary>
        public event EventHandler<EventArgs> DataSent;

        /// <summary>
        /// Occurs when data was received successfully.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Occurs in any case of network errors.
        /// </summary>
        public event EventHandler<NetworkErrorEventArgs> Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpSocketWrapper"/> class.
        /// </summary>
        public UdpSocketWrapper()
        {
            // operation arguments
            var args = new SocketAsyncEventArgs();
            args.Completed += SocketAsyncEventArgs_Completed;
            _socketOperation = args;
        }

        /// <summary>
        /// Initializes the wrapper with a set of local and remote endpoint data.
        /// </summary>
        /// <param name="localAddress">The local address to use for receive operations.</param>
        /// <param name="localPort">The local port to use for receive operations.</param>
        /// <param name="remoteAddress">The remote address to use for send operations.</param>
        /// <param name="remotePort">The remote port to use for send operations.</param>
        public void Initialize(string localAddress, int localPort, string remoteAddress, int remotePort)
        {
            IPEndPoint localEndPoint = null;
            if (!string.IsNullOrEmpty(localAddress))
            {
                localEndPoint = EndPointHelper.ParseEndPoint(localAddress, localPort);
            }
            var remoteEndPoint = EndPointHelper.ParseEndPoint(remoteAddress, remotePort);

            _logger.Trace("Initializing with local endpoint {0} and remote endpoint {1}",
                localEndPoint != null ? localEndPoint.ToString() : "{none}",
                remoteEndPoint);

            // set the remote endpoint
            _socketOperation.RemoteEndPoint = remoteEndPoint;

            // create socket
            _currentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

#if WINDOWS_PHONE
            // make sure we only use the socket when we have a non-cellular (Wi-Fi or similar) connection
            _currentSocket.SetNetworkRequirement(NetworkSelectionCharacteristics.NonCellular);
#endif

            // hook for the server side of code
            OnInitializedPartial(localEndPoint, remoteEndPoint);
        }

        /// <summary>
        /// This method is necessary due to the linked code sharing between client and server.
        /// </summary>
        partial void OnInitializedPartial(IPEndPoint localEndpoint, IPEndPoint remoteEndPoint);

        /// <summary>
        /// Shuts down the socket and cleans up resources.
        /// </summary>
        public void Shutdown()
        {
            _logger.Trace("Shutting down");

            CleanUp();
        }

        private void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            _logger.Trace("Received Completed event from socket async event args");

            // check for errors
            if (e.SocketError != SocketError.Success)
            {
                _logger.Trace("Raising event Error with {0}", e.SocketError);

                // raise error event
                var handlers = Error;
                if (handlers != null)
                {
                    var message = string.Format("The last socket operation ({0}) failed.", e.LastOperation);
                    var args = new NetworkErrorEventArgs(message, e.SocketError.ToString(), null);
                    handlers(this, args);
                }

                return;
            }

            // check what has been executed
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.SendTo:
                    _logger.Trace("Raising event DataSent");

                    // raise event
                    var sentHandlers = DataSent;
                    if (sentHandlers != null)
                    {
                        sentHandlers(this, EventArgs.Empty);
                    }
                    break;
                case SocketAsyncOperation.ReceiveFrom:
                    _logger.Trace("Raising event DataReceived");

                    // copy the received amount of data to separate array
                    var receivedBytes = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, 0, receivedBytes, 0, e.BytesTransferred);

                    // raise event
                    var receivedHandlers = DataReceived;
                    if (receivedHandlers != null)
                    {
                        receivedHandlers(this, new DataReceivedEventArgs(receivedBytes));
                    }
                    break;
            }
        }

        /// <summary>
        /// Starts an asynchronous receive operation.
        /// </summary>
        public void ReceiveFromAsync()
        {
            Guard();

            _logger.Trace("Receiving");

#if WINDOWS_PHONE
            // this is a workaround for Windows Phone 7 for a bug in the ReceiveFromAsync method
            // see e.g. here http://stackoverflow.com/questions/6551477/issues-with-async-receiving-udp-unicast-packets-in-windows-phone-7
            if (!_receiveWorkaroundApplied)
            {
                ApplyReceiveWorkaround();
            }
#endif

            // start receiving
            _socketOperation.SetBuffer(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);
            bool completesAsynchronously = _currentSocket.ReceiveFromAsync(_socketOperation);

            if (!completesAsynchronously)
            {
                SocketAsyncEventArgs_Completed(_currentSocket, _socketOperation);
            }
        }

#if WINDOWS_PHONE
        #region Workaround for a ReceiveFromAsync bug

        private bool _receiveWorkaroundApplied;

        private void ApplyReceiveWorkaround()
        {
            _logger.Trace("Applying receive workaround for WP7");

            var waitEvent = new ManualResetEvent(false);
            var tempEventArgs = new SocketAsyncEventArgs();
            tempEventArgs.SetBuffer(new byte[] { 0xff }, 0, 1);
            tempEventArgs.RemoteEndPoint = _socketOperation.RemoteEndPoint;
            tempEventArgs.Completed += (s, e) => waitEvent.Set();
            _currentSocket.SendToAsync(tempEventArgs);
            waitEvent.WaitOne();
            waitEvent.Dispose();
            tempEventArgs.Dispose();

            _receiveWorkaroundApplied = true;
        }
        #endregion
#endif

        /// <summary>
        /// Starts an asynchronous send operation.
        /// </summary>
        /// <param name="data">The data that should be sent.</param>
        public void SendToAsync(byte[] data)
        {
            Guard();

            _logger.Trace("Sending data");

            // send!
            _socketOperation.SetBuffer(data, 0, data.Length);
            bool completesAsynchronously = _currentSocket.SendToAsync(_socketOperation);

            // check if the completed event will be raised.
            // if not, invoke the handler manually.
            if (!completesAsynchronously)
            {
                SocketAsyncEventArgs_Completed(_currentSocket, _socketOperation);
            }
        }

        private void Guard()
        {
            if (_socketOperation == null || _socketOperation.RemoteEndPoint == null)
            {
                throw new InvalidOperationException("Socket needs to be created and initialized.");
            }
        }

        private void CleanUp()
        {
            _logger.Trace("Cleaning up");

            if (_currentSocket != null)
            {
                try
                {
                    _currentSocket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {
                    // ignore
                }

                _currentSocket.Close();
                _currentSocket = null;
            }
        }
    }
}
