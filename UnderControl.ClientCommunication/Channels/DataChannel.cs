using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnderControl.Shared;
using UnderControl.Shared.Channels;
using UnderControl.Shared.Data;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication.Channels
{
    /// <summary>
    /// This is an implementation of the <c>IDataChannel</c> interface.
    /// </summary>
    internal sealed partial class DataChannel : ChannelBase, IDataChannel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int QueuePurgeThreshold = 10;

        private IUdpSocketWrapper _udpSender;
        private readonly Queue<IDataMessage> _messageQueue = new Queue<IDataMessage>();
        private bool _isSending;
        private DateTime _lastMessageSent;

#pragma warning disable 67 // this warning only is produced due to the code sharing setup
        /// <summary>
        /// Occurs when a data message was successfully received, on the server side.
        /// </summary>
        public event EventHandler<DataMessageEventArgs> DataMessageReceived;
#pragma warning restore 67

        /// <summary>
        /// Gets or sets the minimum number of milliseconds between messages.
        /// Messages are delayed by the amount given here if necessary.
        /// </summary>
        /// <value>
        /// The minimum number of milliseconds between messages.
        /// </value>
        public int MinMillisecondsBetweenMessages
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataChannel"/> class.
        /// </summary>
        public DataChannel()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataChannel"/> class using the given communication factory.
        /// </summary>
        /// <param name="communicationFactory">The communication factory that is used to create the actual communication objects.</param>
        public DataChannel(ICommunicationFactory communicationFactory)
            : base(communicationFactory)
        {
        }

        protected override void OnConnectAsync(IPEndPoint remoteEndPoint)
        {
            _logger.Trace("Extending initialization of base class");

            // create corresponding UDP socket
            _udpSender = CommunicationFactory.CreateUdpSocket();
            _udpSender.Initialize(null, 0, remoteEndPoint.Address.ToString(), remoteEndPoint.Port); // we don't know and don't need a local endpoint for the client
            _udpSender.DataSent += Sender_DataSent;

            Socket.DataSent += Sender_DataSent;
        }

        protected override void OnCleaningUp()
        {
            _logger.Trace("Extending clean up of base class");

            OnCleaningUpPartial();

            if (_udpSender != null)
            {
                _udpSender.DataSent -= Sender_DataSent;
                _udpSender.Shutdown();
                _udpSender = null;
            }

            if (Socket != null)
            {
                Socket.DataSent -= Sender_DataSent;
            }

            lock (_messageQueue)
            {
                // reset 
                _isSending = false;
                _messageQueue.Clear();
            }
        }

        partial void OnCleaningUpPartial();

        private void Sender_DataSent(object sender, EventArgs e)
        {
            _logger.Trace("Received DataSent event from sending socket");

            lock (_messageQueue)
            {
                _isSending = false;

                if (_messageQueue.Count > 0)
                {
                    // check the number of messages that have piled up
                    while (_messageQueue.Count > QueuePurgeThreshold)
                    {
                        var peekMessage = _messageQueue.Peek();
                        if (peekMessage.MustBeDelivered)
                        {
                            break;
                        }
                        else
                        {
                            // throw away oldest messages
                            _messageQueue.Dequeue();
                        }
                    }

                    var nextMessage = _messageQueue.Dequeue();
                    SendDataMessage(nextMessage);
                }
            }
        }

        /// <summary>
        /// Sends the specified data message to the server.
        /// </summary>
        /// <param name="data">The data message to send.</param>
        public void Send(IDataMessage data)
        {
            Guard();

            _logger.Trace("Send request for data {0}", data.DataType);

            lock (_messageQueue)
            {
                if (_isSending)
                {
                    _messageQueue.Enqueue(data);
                }
                else
                {
                    SendDataMessage(data);
                }
            }
        }

        private void SendDataMessage(IDataMessage data)
        {
            _logger.Trace("Sending data message {0}", data.DataType);

            _isSending = true;
            var rawData = data.ToByteArray();

            // take a look whether we need to throttle sending
            if (MinMillisecondsBetweenMessages > 0)
            {
                var now = DateTime.Now;
                var elapsed = now - _lastMessageSent;
                var diff = MinMillisecondsBetweenMessages - (int)Math.Floor(elapsed.TotalMilliseconds);

                if (diff > 0)
                {
                    Thread.Sleep(diff);
                }
            }

            if (data.MustBeDelivered)
            {
                Socket.SendAsync(rawData);
            }
            else
            {
                _udpSender.SendToAsync(rawData);
            }

            // store reference time
            _lastMessageSent = DateTime.Now;
        }

        /// <summary>
        /// Purges a specific type of data messages from the send queue.
        /// </summary>
        /// <param name="dataType">The type of the data to purge from the send queue.</param>
        public void PurgeFromQueue(DataType dataType)
        {
            _logger.Trace("Purging data type {0} from message queue", dataType);

            lock (_messageQueue)
            {
                var tempList = new List<IDataMessage>(_messageQueue.Count);

                // get all messages that don't have the type to purge
                while (_messageQueue.Count > 0)
                {
                    var message = _messageQueue.Dequeue();
                    if (message.DataType != dataType)
                    {
                        tempList.Add(message);
                    }
                }

                // re-fill the queue
                for (int i = 0; i < tempList.Count; i++)
                {
                    _messageQueue.Enqueue(tempList[i]);
                }
            }
        }

        private void Guard()
        {
            if (Socket == null || !IsConnected)
            {
                throw new InvalidOperationException("Cannot send when socket is not created and connected.");
            }
        }
    }
}
