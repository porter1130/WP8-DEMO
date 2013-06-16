
using System.Net;
using System.Net.Sockets;
using UnderControl.Communication.Helpers;
using UnderControl.Shared.Channels;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication.Channels
{
    internal abstract partial class ChannelBase : IChannel
    {
        partial void OnCreatedSocketPartial()
        {
            Socket.ClientAccepted += Socket_ClientAccepted;
        }

        /// <summary>
        /// Starts asynchronous listening for incoming client connections.
        /// </summary>
        /// <param name="localAddress">The local address used for the listening.</param>
        /// <param name="port">The port used for the listening.</param>
        public void ListenAsync(string localAddress, int port)
        {
            var localEndPoint = EndPointHelper.ParseEndPoint(localAddress, port);
            LocalEndpoint = localEndPoint;

            _logger.Trace("Listening async with local endpoint {0}", localEndPoint);

            EnsureSocket();

            OnListenAsync(localEndPoint);

            // build local endpoint
            Socket.ListenAsync(LocalEndpoint.Address.ToString(), LocalEndpoint.Port);
        }

        protected virtual void OnListenAsync(IPEndPoint localEndPoint)
        {
        }

        private void Socket_ClientAccepted(object sender, ClientAcceptedEventArgs e)
        {
            _logger.Trace("Received ClientAccepted event from socket with remote endpoint {0}", e.RemoteAddress);

            OnClientAccepted(e.RemoteAddress, e.RemotePort);

            _logger.Trace("Raising ClientAccepted event");

            var handlers = ClientAccepted;
            if (handlers != null)
            {
                handlers(this, e);
            }
        }

        protected virtual void OnClientAccepted(string remoteAddress, int remotePort)
        {
        }

        partial void OnCleaningUpPartial()
        {
            if (Socket != null)
            {
                // only unhook our own event handler
                Socket.ClientAccepted -= Socket_ClientAccepted;
            }
        }
    }
}
