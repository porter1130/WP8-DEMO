using System;

namespace UnderControl.Shared.Sockets
{
    /// <summary>
    /// Event arguments that transport information about a discovered server on the same network.
    /// </summary>
    public class ServerDiscoveredEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the address of the discovered server.
        /// </summary>
        /// <value>
        /// The address of the discovered server.
        /// </value>
        public string RemoteAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDiscoveredEventArgs"/> class using a given server address.
        /// </summary>
        /// <param name="remoteAddress">The server address.</param>
        public ServerDiscoveredEventArgs(string remoteAddress)
        {
            RemoteAddress = remoteAddress;
        }
    }
}
