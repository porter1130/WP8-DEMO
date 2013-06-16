using System.Net;
using UnderControl.Communication.Sockets;
using UnderControl.Shared.Channels;
using UnderControl.Shared.Data;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication.Channels
{
    internal sealed partial class DataChannel : IDataChannel
    {
        private UdpSocketWrapper _udpReceiver;

        protected override void OnListenAsync(IPEndPoint localEndPoint)
        {
            _logger.Trace("Extending base class initialization");

            Socket.DataReceived += Socket_DataReceived;
        }

        protected override void OnClientAccepted(string remoteAddress, int remotePort)
        {
            _logger.Trace("Extending base class client accept method");

            // at this point, we know the client ip
            // => create udp socket
            _udpReceiver = new UdpSocketWrapper();
            _udpReceiver.Initialize(LocalEndpoint.Address.ToString(),
                LocalEndpoint.Port,
                remoteAddress,
                remotePort);
            _udpReceiver.DataReceived += Socket_DataReceived;
            _udpReceiver.ReceiveFromAsync();

            // start receiving
            Socket.ReceiveAsync();
        }

        private void Socket_DataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.Trace("Received DataReceived event from socket");

            // check what socket type we have
            var socket = sender as TcpSocketWrapper;
            if (socket != null)
            {
                // let the base class handle fragments
                ProcessReceivedDataFragment(e.Data, (data, startIndex) =>
                {
                    // treat restored messages as data messages
                    var fragmentDataMessage = DataMessageFactory.CreateFromRawMessage(data, startIndex);
                    RaiseDataMessageReceivedEvent(fragmentDataMessage);
                });

                // start over
                socket.ReceiveAsync();
            }
            else
            {
                // udp
                var udpSocket = sender as UdpSocketWrapper;
                if (udpSocket != null)
                {
                    // udp does not fragment
                    var dataMessage = DataMessageFactory.CreateFromRawMessage(e.Data);
                    RaiseDataMessageReceivedEvent(dataMessage);

                    // start over
                    udpSocket.ReceiveFromAsync();
                }
            }
        }

        private void RaiseDataMessageReceivedEvent(DataMessage dataMessage)
        {
            _logger.Trace("Raising DataMessageReceived event");

            var handlers = DataMessageReceived;
            if (handlers != null)
            {
                handlers(this, new DataMessageEventArgs(dataMessage));
            }
        }

        partial void OnCleaningUpPartial()
        {
            _logger.Trace("Partial clean up method to shut down UDP receiver");

            if (Socket != null)
            {
                Socket.DataReceived -= Socket_DataReceived;
            }

            if (_udpReceiver != null)
            {
                _udpReceiver.DataReceived -= Socket_DataReceived;
                _udpReceiver.Shutdown();
                _udpReceiver = null;
            }
        }
    }
}
