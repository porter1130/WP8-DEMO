using System;

namespace UnderControl.Shared.Sockets
{
    /// <summary>
    /// An interface that describes a convenient wrapper around a low-level TCP socket.
    /// </summary>
    public interface ITcpSocketWrapper
    {
        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the socket is connected; otherwise, <c>false</c>.
        /// </value>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Occurs when the socket was connected successfully.
        /// </summary>
        event EventHandler<EventArgs> Connected;

        /// <summary>
        /// Occurs when the connection was closed successfully.
        /// </summary>
        event EventHandler<EventArgs> ConnectionClosed;

        /// <summary>
        /// Occurs when a client was accepted successfully.
        /// </summary>
        event EventHandler<ClientAcceptedEventArgs> ClientAccepted;

        /// <summary>
        /// Occurs when data was sent successfully.
        /// </summary>
        event EventHandler<EventArgs> DataSent;

        /// <summary>
        /// Occurs when data was received successfully.
        /// </summary>
        event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Occurs in any case of network errors.
        /// </summary>
        event EventHandler<NetworkErrorEventArgs> Error;

        /// <summary>
        /// Starts an asnychronous listen operation for incoming client connect requests.
        /// </summary>
        /// <param name="localAddress">The local address to use for the listen operation.</param>
        /// <param name="localPort">The local port to use for the listen operation.</param>
        void ListenAsync(string localAddress, int localPort);

        /// <summary>
        /// Starts an asynchronous connect operation to connect to a remote server.
        /// </summary>
        /// <param name="remoteAddress">The remote address to connect to.</param>
        /// <param name="remotePort">The remote port to use during the connection attempt.</param>
        void ConnectAsync(string remoteAddress, int remotePort);

        /// <summary>
        /// Starts an asynchronous receive operation.
        /// </summary>
        void ReceiveAsync();

        /// <summary>
        /// Starts an asynchronous send operations
        /// </summary>
        /// <param name="data">The data that should be sent.</param>
        void SendAsync(byte[] data);

        /// <summary>
        /// Shuts down the socket.
        /// </summary>
        void Shutdown();
    }
}