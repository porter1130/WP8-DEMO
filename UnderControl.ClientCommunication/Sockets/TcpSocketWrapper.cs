using System;
using System.Net.Sockets;
using UnderControl.Communication.Helpers;
using UnderControl.Shared;
using UnderControl.Shared.Sockets;


#if WINDOWS_PHONE
using Microsoft.Phone.Net.NetworkInformation;
#endif

namespace UnderControl.Communication.Sockets
{
    /// <summary>
    /// An implementation of the <c>ITcpSocketWrapper</c> interface.
    /// </summary>
    internal partial class TcpSocketWrapper : ITcpSocketWrapper
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int ReceiveBufferSize = 1024;

        private readonly SocketAsyncEventArgs _socketOperation;
        private Socket _currentSocket;

        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the socket is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                return _currentSocket != null && _currentSocket.Connected;
            }
        }

        /// <summary>
        /// Occurs when the socket was connected successfully.
        /// </summary>
        public event EventHandler<EventArgs> Connected;

        /// <summary>
        /// Occurs when the connection was closed successfully.
        /// </summary>
        public event EventHandler<EventArgs> ConnectionClosed;

#pragma warning disable 67 // this warning only is produced due to the code sharing setup
        /// <summary>
        /// Occurs when a client was accepted successfully.
        /// </summary>
        public event EventHandler<ClientAcceptedEventArgs> ClientAccepted;
#pragma warning restore 67

        /// <summary>
        /// Occurs when data was sent successfully.
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
        /// Initializes a new instance of the <see cref="TcpSocketWrapper"/> class.
        /// </summary>
        public TcpSocketWrapper()
        {
            // operation arguments
            var args = new SocketAsyncEventArgs();
            args.Completed += SocketAsyncEventArgs_Completed;
            _socketOperation = args;
        }

        /// <summary>
        /// Starts an asynchronous connect operation to connect to a remote server.
        /// </summary>
        /// <param name="remoteAddress">The remote address to connect to.</param>
        /// <param name="remotePort">The remote port to use during the connection attempt.</param>
        public void ConnectAsync(string remoteAddress, int remotePort)
        {
            var remoteEndPoint = EndPointHelper.ParseEndPoint(remoteAddress, remotePort);

            _logger.Trace("Connecting to remote endpoint {0}", remoteEndPoint);

            var socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

#if WINDOWS_PHONE
            // make sure we only use the socket when we have a non-cellular (Wi-Fi or similar) connection
            socket.SetNetworkRequirement(NetworkSelectionCharacteristics.NonCellular);
#endif

            // update operation arguments
            _socketOperation.RemoteEndPoint = remoteEndPoint;

            // connect socket
            bool completesAsynchronously = socket.ConnectAsync(_socketOperation);

            // check if the completed event will be raised.
            // if not, invoke the handler manually.
            if (!completesAsynchronously)
            {
                SocketAsyncEventArgs_Completed(_socketOperation.ConnectSocket, _socketOperation);
            }
        }

        /// <summary>
        /// Shuts down the socket.
        /// </summary>
        public void Shutdown()
        {
            _logger.Trace("Shutting down");

            CleanUp();
        }

        private void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            _logger.Trace("Received Completed event from socket event args");

            // check for errors
            if (e.SocketError != SocketError.Success)
            {
                _logger.Trace("Raising event Error with code {0}", e.SocketError);

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
                case SocketAsyncOperation.Connect:
                    // store connect socket
                    _currentSocket = e.ConnectSocket;

                    _logger.Trace("Raising event Connected");

                    // raise event
                    var connectedHandlers = Connected;
                    if (connectedHandlers != null)
                    {
                        connectedHandlers(this, EventArgs.Empty);
                    }
                    break;
                case SocketAsyncOperation.Send:
                    // if zero bytes were transferred, the remote endpoint has closed the connection
                    if (e.BytesTransferred == 0)
                    {
                        RaiseConnectionClosedEvent();
                        break;
                    }

                    _logger.Trace("Raising event DataSent");

                    // raise event
                    var sentHandlers = DataSent;
                    if (sentHandlers != null)
                    {
                        sentHandlers(this, EventArgs.Empty);
                    }
                    break;
                case SocketAsyncOperation.Receive:
                    // if zero bytes were transferred, the remote endpoint has closed the connection
                    if (e.BytesTransferred == 0)
                    {
                        RaiseConnectionClosedEvent();
                        break;
                    }

                    // copy the received amount of data to separate array
                    var receivedBytes = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, 0, receivedBytes, 0, e.BytesTransferred);

                    _logger.Trace("Raising event DataReceived");

                    // raise event
                    var receivedHandlers = DataReceived;
                    if (receivedHandlers != null)
                    {
                        receivedHandlers(this, new DataReceivedEventArgs(receivedBytes));
                    }
                    break;
                default:
                    OnUnknownOperationCompleted(sender, e);
                    break;
            }
        }

        private void RaiseConnectionClosedEvent()
        {
            _logger.Trace("Raising event ConnectionClosed");

            // raise event
            var handlers = ConnectionClosed;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// This method is necessary due to the linked code sharing between client and server.
        /// </summary>
        partial void OnUnknownOperationCompleted(object sender, SocketAsyncEventArgs e);

        /// <summary>
        /// Starts an asynchronous receive operation.
        /// </summary>
        public void ReceiveAsync()
        {
            Guard();

            _logger.Trace("Receiving");

            // start receiving
            _socketOperation.SetBuffer(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);
            bool completesAsynchronously = _currentSocket.ReceiveAsync(_socketOperation);

            if (!completesAsynchronously)
            {
                SocketAsyncEventArgs_Completed(_currentSocket, _socketOperation);
            }
        }

        /// <summary>
        /// Starts an asynchronous send operations
        /// </summary>
        /// <param name="data">The data that should be sent.</param>
        public void SendAsync(byte[] data)
        {
            Guard();

            _logger.Trace("Sending data");

            // send!
            _socketOperation.SetBuffer(data, 0, data.Length);
            bool completesAsynchronously = _currentSocket.SendAsync(_socketOperation);

            // check if the completed event will be raised.
            // if not, invoke the handler manually.
            if (!completesAsynchronously)
            {
                SocketAsyncEventArgs_Completed(_currentSocket, _socketOperation);
            }
        }

        private void Guard()
        {
            if (_socketOperation == null || !IsConnected)
            {
                throw new InvalidOperationException("Socket needs to be created and connected.");
            }
        }

        private void CleanUp()
        {
            _logger.Trace("Cleaning up");

            if (_socketOperation != null)
            {
                if (_socketOperation.ConnectSocket != null)
                {
                    if (_socketOperation.ConnectSocket.Connected)
                    {
                        try
                        {
                            _socketOperation.ConnectSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch (SocketException)
                        {
                            // this may happen if the other side has already closed the connection
                        }
                    }
                    _socketOperation.ConnectSocket.Close();
                }

                OnCleanUpPartial();
            }
        }

        /// <summary>
        /// This method is necessary due to the linked code sharing between client and server.
        /// </summary>
        partial void OnCleanUpPartial();

#if WINDOWS_PHONE

        /// <summary>
        /// This method is not supported on the phone and throws a <c>NotSupportedException</c> when it's used.
        /// </summary>
        public void ListenAsync(string localAddress, int localPort)
        {
            throw new NotSupportedException("ListenAsync is not supported on the phone.");
        }

#endif

    }
}
