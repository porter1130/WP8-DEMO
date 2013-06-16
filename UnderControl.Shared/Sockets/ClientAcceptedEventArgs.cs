using System;

namespace UnderControl.Shared.Sockets
{
    /// <summary>
    /// Event arguments that transport information about an accepted client connection.
    /// </summary>
    public class ClientAcceptedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the remote address of the accepted client.
        /// </summary>
        public string RemoteAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the remote port of the accepted client.
        /// </summary>
        public int RemotePort
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientAcceptedEventArgs"/> class using the given address and port of the remote client.
        /// </summary>
        /// <param name="remoteAddress">The remote address of the accepted client.</param>
        /// <param name="remotePort">The remote port of the accepted client.</param>
        public ClientAcceptedEventArgs(string remoteAddress, int remotePort)
        {
            RemoteAddress = remoteAddress;
            RemotePort = remotePort;
        }
    }
}