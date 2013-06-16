using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnderControl.Shared;

namespace UnderControl.Communication.Sockets
{
    /// <summary>
    /// A multicast listener implementation that allows the server side to listen for client broadcasts.
    /// </summary>
    internal sealed class MulticastListener
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private Socket _receiver;
        private Socket _sender;

        /// <summary>
        /// Starts listening to incoming client broadcasts.
        /// </summary>
        public void StartListening()
        {
            ThreadPool.QueueUserWorkItem(Listen);
        }

        /// <summary>
        /// Shuts down the multicast listener.
        /// </summary>
        public void Shutdown()
        {
            _logger.Trace("Shutting down");

            // multicast ip address
            IPAddress ip = IPAddress.Parse(Constants.MulticastGroupAddress);

            if (_receiver != null)
            {
                _receiver.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(ip, IPAddress.Any));
                _receiver.Shutdown(SocketShutdown.Both);
                _receiver.Close();
                _receiver = null;
            }

            if (_sender != null)
            {
                _sender.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(ip, IPAddress.Any));
                _sender.Shutdown(SocketShutdown.Both);
                _sender.Close();
                _sender = null;
            }
        }

        private void Listen(object state)
        {
            _logger.Trace("Listening");

            InitializeReceiver();
            InitializeSender();

            // receive
            Receive();
        }

        private void InitializeReceiver()
        {
            _logger.Trace("Initializing receiver");

            // create the socket
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // bind
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.MulticastGroupPort);
            socket.Bind(localEndPoint);

            // multicast ip address
            IPAddress ip = IPAddress.Parse(Constants.MulticastGroupAddress);

            // join multicast group
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));
            socket.MulticastLoopback = false;

            _receiver = socket;
        }

        private void InitializeSender()
        {
            _logger.Trace("Initializing sender");

            // create the socket
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // multicast ip address
            IPAddress ip = IPAddress.Parse(Constants.MulticastGroupAddress);

            // connect the socket
            IPEndPoint endPoint = new IPEndPoint(ip, Constants.MulticastGroupPort);
            socket.Connect(endPoint);

            // join multicast group
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
            socket.MulticastLoopback = false;

            _sender = socket;
        }

        private void Receive()
        {
            _logger.Trace("Receiving");

            byte[] buffer = new byte[Constants.MulticastMaxMessageSize];

            try
            {
                int readBytes = _receiver.Receive(buffer);

                while (readBytes > 0)
                {
                    string result = Encoding.UTF8.GetString(buffer, 0, readBytes);

                    // simply bounce the message to signal to the client that we are indeed the server they are looking for
                    if (result == Constants.MulticastDiscoveryToken)
                    {
                        _sender.Send(buffer, 0, Constants.MulticastDiscoveryToken.Length, SocketFlags.None);
                    }

                    readBytes = _receiver.Receive(buffer);
                }
            }
            catch (SocketException)
            {
                // happens when we shut down
                return;
            }
        }
    }
}
