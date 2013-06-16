using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnderControl.Shared;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication.Sockets
{
    /// <summary>
    /// A multicast client used to broadcast to discover servers on the same network.
    /// </summary>
    internal class MulticastClient : IMulticastClient
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // a client receiver for multicast traffic from any source
        private UdpAnySourceMulticastClient _client;

        // workaround for the fact that there's no timeout setting and the client just
        // seems to wait indefinitely for a receive
        private Timer _timer;

        // some flags
        private bool _joined;
        private bool _receiving;

        // buffer for incoming data
        private readonly byte[] _receiveBuffer = new byte[Constants.MulticastMaxMessageSize];

        /// <summary>
        /// Occurs when a server has been discovered.
        /// </summary>
        public event EventHandler<ServerDiscoveredEventArgs> ServerDiscovered;

        /// <summary>
        /// Occurs when a timeout has been detected.
        /// </summary>
        public event EventHandler<EventArgs> TimeoutElapsed;

        /// <summary>
        /// Starts discovery of servers on the same network.
        /// </summary>
        public void DiscoverServer()
        {
            _logger.Trace("Discovering server");

            if (!_joined)
            {
                // start with joining the group
                Join();
            }
            else
            {
                // we're already joined => broadcast
                BroadcastToServer();
            }
        }

        /// <summary>
        /// Shuts down the multicast client.
        /// </summary>
        public void Shutdown()
        {
            _logger.Trace("Shutting down");

            CleanUp();
        }

        /// <summary>
        /// Create a new UdpAnySourceMulticastClient instance and join the group.
        /// </summary>
        private void Join()
        {
            _logger.Trace("Joining multicast group");

            // Create the UdpAnySourceMulticastClient instance using the defined 
            // GROUP_ADDRESS and GROUP_PORT constants. UdpAnySourceMulticastClient is a 
            // client receiver for multicast traffic from any source, also known as Any Source Multicast (ASM)
            if (_client == null)
            {
                _client = new UdpAnySourceMulticastClient(IPAddress.Parse(Constants.MulticastGroupAddress), Constants.MulticastGroupPort);
            }

            // Make a request to join the group.
            _client.BeginJoinGroup(
                result =>
                {
                    // Complete the join
                    _client.EndJoinGroup(result);

                    // The MulticastLoopback property controls whether you receive multicast 
                    // packets that you send to the multicast group. Default value is true, 
                    // meaning that you also receive the packets you send to the multicast group. 
                    // To stop receiving these packets, you can set the property following to false
                    _client.MulticastLoopback = false;

                    // Set a flag indicating that we have now joined the multicast group 
                    _joined = true;

                    // broadcast
                    BroadcastToServer();
                }, null);
        }

        private void BroadcastToServer()
        {
            _logger.Trace("Broadcasting to server");

            // let's broadcast to see whether we have a server in the group
            Send(Constants.MulticastDiscoveryToken);

            if (!_receiving)
            {
                // Wait for data from the group. This is an asynchronous operation 
                // and will not block the UI thread.
                Receive();
            }

            // create a timer if necessary and change the timeout each time
            // we broadcast
            if (_timer == null)
            {
                _timer = new Timer(Timer_Tick);
            }
            _timer.Change(Constants.MulticastTimeout, Timeout.Infinite);
        }

        private void Timer_Tick(object state)
        {
            // disable, just to be sure
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            if (_client == null)
            {
                // we have been cleaned up in the meantime
                return;
            }

            _logger.Trace("Timeout elapsed");

            // shut down
            Shutdown();

            RaiseTimeoutElapsedEvent();
        }

        /// <summary>
        /// Send the given message to the multicast group.
        /// </summary>
        /// <param name="message">The message to send</param>
        private void Send(string message)
        {
            _logger.Trace("Sending message to multicast group: {0}", message);

            // Attempt the send only if you have already joined the group.
            if (_joined)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _client.BeginSendToGroup(data, 0, data.Length,
                    result =>
                    {
                        _client.EndSendToGroup(result);
                    }, null);
            }
        }

        /// <summary>
        /// Receive data from the group and log it.
        /// </summary>
        private void Receive()
        {
            _logger.Trace("Receiving from multicast group");

            // Only attempt to receive if you have already joined the group
            if (_joined)
            {
                Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);

                // receive
                _client.BeginReceiveFromGroup(_receiveBuffer, 0, _receiveBuffer.Length,
                    result =>
                    {
                        IPEndPoint source;

                        // if we ran into a timeout, the client is cleaned up already
                        if (_client == null)
                        {
                            return;
                        }

                        // Complete the asynchronous operation. The source field will 
                        // contain the IP address of the device that sent the message
                        _client.EndReceiveFromGroup(result, out source);

                        // Get the received data from the buffer.
                        string dataReceived = Encoding.UTF8.GetString(_receiveBuffer, 0, _receiveBuffer.Length).Trim('\0');

                        if (dataReceived == Constants.MulticastDiscoveryToken)
                        {
                            CleanUp();
                            RaiseServerDiscoveredEvent(source.Address);
                        }
                        else
                        {
                            // Call receive again to continue to "listen" for the next message from the group
                            Receive();
                        }
                    }, null);

                // set flag
                _receiving = true;
            }
        }

        private void CleanUp()
        {
            _logger.Trace("Cleaning up");

            _receiving = false;
            _joined = false;

            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }

        private void RaiseServerDiscoveredEvent(IPAddress serverIp)
        {
            var address = serverIp.ToString();
            _logger.Trace("Raising ServerDiscovered event for {0}", address);

            var handlers = ServerDiscovered;
            if (handlers != null)
            {
                handlers(this, new ServerDiscoveredEventArgs(address));
            }
        }

        private void RaiseTimeoutElapsedEvent()
        {
            _logger.Trace("Raising TimeoutElapsed event");

            var handlers = TimeoutElapsed;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }
    }
}
