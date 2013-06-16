using System;

namespace UnderControl.Shared.Sockets
{
    /// <summary>
    /// An interface that describes a multicast client.
    /// </summary>
    public interface IMulticastClient
    {
        /// <summary>
        /// Occurs when a server has been discovered.
        /// </summary>
        event EventHandler<ServerDiscoveredEventArgs> ServerDiscovered;

        /// <summary>
        /// Occurs when a timeout has been detected.
        /// </summary>
        event EventHandler<EventArgs> TimeoutElapsed;

        /// <summary>
        /// Starts discovery of servers on the same network.
        /// </summary>
        void DiscoverServer();

        /// <summary>
        /// Shuts down the multicast client.
        /// </summary>
        void Shutdown();
    }
}