using System;

namespace UnderControl.Shared.Sockets
{
    /// <summary>
    /// An interface that describes a convenient wrapper around a low-level UDP socket.
    /// </summary>
    public interface IUdpSocketWrapper
    {
        /// <summary>
        /// Occurs when data was sent successfully.
        /// Due to the nature of UDP, this does not mean that the data was successfully received on the other end.
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
        /// Initializes the wrapper with a set of local and remote endpoint data.
        /// </summary>
        /// <param name="localAddress">The local address to use for receive operations.</param>
        /// <param name="localPort">The local port to use for receive operations.</param>
        /// <param name="remoteAddress">The remote address to use for send operations.</param>
        /// <param name="remotePort">The remote port to use for send operations.</param>
        void Initialize(string localAddress, int localPort, string remoteAddress, int remotePort);

        /// <summary>
        /// Starts an asynchronous receive operation.
        /// </summary>
        void ReceiveFromAsync();

        /// <summary>
        /// Starts an asynchronous send operation.
        /// </summary>
        /// <param name="data">The data that should be sent.</param>
        void SendToAsync(byte[] data);

        /// <summary>
        /// Shuts down the socket and cleans up resources.
        /// </summary>
        void Shutdown();
    }
}