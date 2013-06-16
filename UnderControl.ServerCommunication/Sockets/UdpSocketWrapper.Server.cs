using System.Net;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication.Sockets
{
    internal partial class UdpSocketWrapper : IUdpSocketWrapper
    {
        partial void OnInitializedPartial(IPEndPoint localEndpoint, IPEndPoint remoteEndPoint)
        {
            _logger.Trace("Extending OnInitialized to bind receiving socket");

            if (localEndpoint != null)
            {
                // this needs to be done for receiving sockets on the server
                _currentSocket.Bind(localEndpoint);
            }
        }
    }
}
