using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnderControl.Communication.Helpers;
using UnderControl.Shared;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication.Sockets
{
    internal partial class TcpSocketWrapper : ITcpSocketWrapper
    {
        private Socket _listener;

        /// <summary>
        /// Starts an asnychronous listen operation for incoming client connect requests.
        /// </summary>
        /// <param name="localAddress">The local address to use for the listen operation.</param>
        /// <param name="localPort">The local port to use for the listen operation.</param>
        public void ListenAsync(string localAddress, int localPort)
        {
            var localEndPoint = EndPointHelper.ParseEndPoint(localAddress, localPort);

            _logger.Trace("Listening (async) using local endpoint {0}", localEndPoint);

            ThreadPool.QueueUserWorkItem(o => Listen(localEndPoint), null);
        }

        private void Listen(IPEndPoint localEndpoint)
        {
            _logger.Trace("Listen (sync) using local endpoint {0}", localEndpoint);

            // create a socket
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // bind and listen
            try
            {
                _listener.Bind(localEndpoint);
                _listener.Listen(1);

                // accept the next connection
                bool completesAsynchronously = _listener.AcceptAsync(_socketOperation);

                if (!completesAsynchronously)
                {
                    SocketAsyncEventArgs_Completed(_socketOperation.AcceptSocket, _socketOperation);
                }
            }
            catch (Exception ex)
            {
                // raise error event
                var handlers = Error;
                if (handlers != null)
                {
                    var args = new NetworkErrorEventArgs("The listen operation failed: " + ex.ToString(), null, ex);
                    handlers(this, args);
                }
            }
        }

        partial void OnUnknownOperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            _logger.Trace("Extending OnUnknownOperationCompleted, operation: ", e.LastOperation);

            // check what has been executed
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    // store accept socket
                    _currentSocket = e.AcceptSocket;

                    // raise event
                    var clientAcceptedHandlers = ClientAccepted;
                    if (clientAcceptedHandlers != null)
                    {
                        var ipEndPoint = (IPEndPoint)_currentSocket.RemoteEndPoint;
                        clientAcceptedHandlers(this, new ClientAcceptedEventArgs(ipEndPoint.Address.ToString(), ipEndPoint.Port));
                    }
                    break;
            }
        }

        partial void OnCleanUpPartial()
        {
            _logger.Trace("Extending OnCustomCleanUp");

            if (_listener != null)
            {
                if (_listener.Connected)
                {
                    try
                    {
                        _listener.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException)
                    {
                        // ignore
                    }
                }
                _listener.Close();
                _listener = null;
            }

            if (_socketOperation != null && _socketOperation.AcceptSocket != null)
            {
                if (_socketOperation.AcceptSocket.Connected)
                {
                    try
                    {
                        _socketOperation.AcceptSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException)
                    {
                        // this may happen if the other side has already closed the connection
                    }
                }
                _socketOperation.AcceptSocket.Close();
            }
        }
    }
}
