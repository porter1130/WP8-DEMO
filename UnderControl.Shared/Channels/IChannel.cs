using System;
using UnderControl.Shared.Sockets;

namespace UnderControl.Shared.Channels
{
    /// <summary>
    /// An interface describing an abstracted communication channel between a client and server.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// Gets a value indicating whether the channel is connected.
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Occurs when the channel was successfully connected to a server.
        /// </summary>
        event EventHandler<EventArgs> Connected;

        /// <summary>
        /// Occurs when a client was accepted by the channel.
        /// </summary>
        event EventHandler<ClientAcceptedEventArgs> ClientAccepted;

        /// <summary>
        /// Occurs when the underlying connection was closed.
        /// </summary>
        event EventHandler<EventArgs> ConnectionClosed;

        /// <summary>
        /// Occurs in case of an error during communication.
        /// </summary>
        event EventHandler<NetworkErrorEventArgs> Error;

        /// <summary>
        /// Starts asynchronous listening for incoming client connections.
        /// </summary>
        /// <param name="localAddress">The local address used for the listening.</param>
        /// <param name="port">The port used for the listening.</param>
        void ListenAsync(string localAddress, int port);

        /// <summary>
        /// Starts asynchronously connecting to a remote server.
        /// </summary>
        /// <param name="remoteAddress">The remote address to connect to.</param>
        /// <param name="remotePort">The remote port to use.</param>
        void ConnectAsync(string remoteAddress, int remotePort);

        /// <summary>
        /// Shuts down the channel.
        /// </summary>
        void Shutdown();
    }
}