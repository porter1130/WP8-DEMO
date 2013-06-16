using System;
using System.Net;
using UnderControl.Communication.Helpers;
using UnderControl.Shared;
using UnderControl.Shared.Channels;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication.Channels
{
    /// <summary>
    /// An implementation of the <c>IChannel</c> interface that is used as base class for the actual channels.
    /// </summary>
    internal abstract partial class ChannelBase : IChannel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the communication factory that is used to create the actual communication objects.
        /// </summary>
        protected ICommunicationFactory CommunicationFactory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the TCP socket wrapper.
        /// </summary>
        protected ITcpSocketWrapper Socket
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the local endpoint used for the channel.
        /// </summary>
        protected IPEndPoint LocalEndpoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the channel is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (Socket != null)
                {
                    return Socket.IsConnected;
                }

                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelBase"/> class.
        /// </summary>
        protected ChannelBase()
        {
            // create a default implementation of the communication factory interface
            CommunicationFactory = new DefaultCommunicationFactory();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelBase"/> class using the given communication factory implementation.
        /// </summary>
        /// <param name="communicationFactory">The communication factory that is used to create the actual communication objects.</param>
        protected ChannelBase(ICommunicationFactory communicationFactory)
        {
            if (communicationFactory == null)
            {
                throw new ArgumentNullException("communicationFactory");
            }

            CommunicationFactory = communicationFactory;
        }

        /// <summary>
        /// Occurs when the channel was successfully connected to a server.
        /// </summary>
        public event EventHandler<EventArgs> Connected;

        /// <summary>
        /// Occurs when the underlying connection was closed.
        /// </summary>
        public event EventHandler<EventArgs> ConnectionClosed;

#pragma warning disable 67 // this warning only is produced due to the code sharing setup
        /// <summary>
        /// Occurs when a client was accepted by the channel.
        /// </summary>
        public event EventHandler<ClientAcceptedEventArgs> ClientAccepted;
#pragma warning restore 67

        /// <summary>
        /// Occurs in case of an error during communication.
        /// </summary>
        public event EventHandler<NetworkErrorEventArgs> Error;

        private void EnsureSocket()
        {
            if (Socket == null)
            {
                Socket = CommunicationFactory.CreateTcpSocket();
                Socket.Connected += Socket_Connected;
                Socket.ConnectionClosed += Socket_ConnectionClosed;
                Socket.Error += Socket_Error;

                OnCreatedSocketPartial();
            }
        }

        /// <summary>
        /// This method is necessary due to the linked code sharing between client and server.
        /// </summary>
        partial void OnCreatedSocketPartial();

        /// <summary>
        /// Starts asynchronously connecting to a remote server.
        /// </summary>
        /// <param name="remoteAddress">The remote address to connect to.</param>
        /// <param name="remotePort">The remote port to use.</param>
        public void ConnectAsync(string remoteAddress, int remotePort)
        {
            var remoteEndPoint = EndPointHelper.ParseEndPoint(remoteAddress, remotePort);

            _logger.Trace("Connecting async with remote endpoint {0}", remoteEndPoint);

            EnsureSocket();

            OnConnectAsync(remoteEndPoint);

            Socket.ConnectAsync(remoteEndPoint.Address.ToString(), remoteEndPoint.Port);
        }

        protected virtual void OnConnectAsync(IPEndPoint remoteEndpoint)
        {
        }

        /// <summary>
        /// Shuts down the channel.
        /// </summary>
        public void Shutdown()
        {
            if (Socket != null)
            {
                _logger.Trace("Shutting down");

                Socket.Shutdown();
                CleanUp();
            }
        }

        private void Socket_Error(object sender, NetworkErrorEventArgs e)
        {
            _logger.Trace("Raising Error event with message: {0}", e.Message);

            // raise our own event
            var handlers = Error;
            if (handlers != null)
            {
                handlers(this, new NetworkErrorEventArgs(e.Message, e.ErrorCode, e.Error));
            }
        }

        private void CleanUp()
        {
            _logger.Trace("Cleaning up");

            OnCleaningUp();
            OnCleaningUpPartial();

            // reset fragment handling 
            _lastFragment = null;
            _lastFragmentLength = 0;

            // clean up socket
            if (Socket != null)
            {
                Socket.Connected -= Socket_Connected;
                Socket.ConnectionClosed -= Socket_ConnectionClosed;
                Socket.Error -= Socket_Error;
                Socket = null;
            }
        }

        /// <summary>
        /// This method is necessary due to the linked code sharing between the client and the server.
        /// </summary>
        partial void OnCleaningUpPartial();

        /// <summary>
        /// Hook for derived classes to do their own cleaning.
        /// </summary>
        protected virtual void OnCleaningUp()
        {
        }

        private void Socket_Connected(object sender, EventArgs e)
        {
            _logger.Trace("Received Connected event from socket");

            OnConnected();

            var handlers = Connected;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        protected virtual void OnConnected()
        {
        }

        private void Socket_ConnectionClosed(object sender, EventArgs e)
        {
            _logger.Trace("Received ConnectionClosed event from socket");

            // simply raise our own event
            var handlers = ConnectionClosed;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        #region Fragmented message handling (receive)

        // used to restore fragmented commands
        private byte[] _lastFragment;
        private int _lastFragmentLength;

        protected void ProcessReceivedDataFragment(byte[] rawData, Action<byte[], int> processRestoredData)
        {
            // Typically, for TCP connections, multiple messages may be transferred in one technical message;
            // What we need to do is consider this, extract message data and/or fragments from what has been received,
            // and assemble fully restored messages that can then be processed by application logic.

            _logger.Trace("Processing received data");

            int startIndex = 0;

            // check if we have a fragment
            if (_lastFragment != null)
            {
                // do we have enough to build the whole command?
                var missingDataLength = _lastFragment.Length - _lastFragmentLength;
                if (rawData.Length < missingDataLength)
                {
                    Array.Copy(rawData, 0, _lastFragment, _lastFragmentLength, rawData.Length);
                    _lastFragmentLength += rawData.Length;

                    // set new start index (which is invalid and will cause no further processing below)
                    startIndex = rawData.Length;
                }
                else
                {
                    // we have enough to build the command
                    Array.Copy(rawData, 0, _lastFragment, _lastFragmentLength, missingDataLength);

                    // process this restored fragment
                    processRestoredData(_lastFragment, 0);
                    //var fragmentCommand = ControlCommandFactory.CreateFromRawCommand(_lastFragment);
                    //RaiseControlCommandReceivedEvent(fragmentCommand);

                    // reset
                    _lastFragment = null;

                    // set new start index (so the processing can continue below)
                    startIndex = missingDataLength;
                }

            }

            // process the (remaining) data
            while (startIndex < rawData.Length)
            {
                // first get length of command
                var dataLength = rawData[startIndex];
                var remainingLength = rawData.Length - startIndex;
                if (dataLength > remainingLength)
                {
                    // this is a fragment => store for next time
                    _lastFragment = new byte[dataLength];
                    _lastFragmentLength = remainingLength;
                    Array.Copy(rawData, startIndex, _lastFragment, 0, remainingLength);
                    break;
                }
                else
                {
                    // we can extract the complete data
                    processRestoredData(rawData, startIndex);
                    //var command = ControlCommandFactory.CreateFromRawCommand(rawData, startIndex);
                    //RaiseControlCommandReceivedEvent(command);

                    // set new start index
                    startIndex += dataLength;
                }
            }
        }

        #endregion

#if WINDOWS_PHONE

        /// <summary>
        /// This method is not supported on the phone and throws a <c>NotSupportedException</c> when it's used.
        /// </summary>
        public void ListenAsync(string localAddress, int port)
        {
            // we need this method to correctly implement the interface,
            // but of course it's not supported on the phone
            throw new NotSupportedException("ListenAsync is not supported on the phone.");
        }

#endif

    }
}
