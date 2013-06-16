using System;
using System.Net;
using UnderControl.Shared;
using UnderControl.Shared.Channels;
using UnderControl.Shared.ControlCommands;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication.Channels
{
    /// <summary>
    /// This is an implementation of the <c>IControlChannel</c> interface.
    /// </summary>
    internal sealed partial class ControlChannel : ChannelBase, IControlChannel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

#pragma warning disable 67 // this warning only is produced due to the code sharing setup
        /// <summary>
        /// Occurs when a control command was successfully sent from the server to the client, on the server side.
        /// </summary>
        public event EventHandler<EventArgs> ControlCommandSent;
#pragma warning restore 67

        /// <summary>
        /// Occurs when a control command was successfully received, on the client side.
        /// </summary>
        public event EventHandler<ControlCommandReceivedEventArgs> ControlCommandReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlChannel"/> class.
        /// </summary>
        public ControlChannel()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlChannel"/> class using the given communication factory.
        /// </summary>
        /// <param name="communicationFactory">The communication factory used to create the actual communication objects.</param>
        public ControlChannel(ICommunicationFactory communicationFactory)
            : base(communicationFactory)
        {
        }

        protected override void OnConnectAsync(IPEndPoint remoteEndpoint)
        {
            _logger.Trace("Extending initialization of base clase");

            Socket.DataReceived += Receiver_DataReceived;
        }

        protected override void OnConnected()
        {
            _logger.Trace("Extending connected of base class");

            Socket.ReceiveAsync();
        }

        protected override void OnCleaningUp()
        {
            _logger.Trace("Extending clean up of base class");

            OnCleaningUpPartial();

            if (Socket != null)
            {
                Socket.DataReceived -= Receiver_DataReceived;
            }
        }

        partial void OnCleaningUpPartial();

        private void Receiver_DataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.Trace("Received DataReceived event from receiving socket");

            // let the base class handle the fragments
            ProcessReceivedDataFragment(e.Data, (data, startIndex) =>
                                                    {
                                                        // treat restored messages as control commands
                                                        var command = ControlCommandFactory.CreateFromRawCommand(data, startIndex);
                                                        RaiseControlCommandReceivedEvent(command);
                                                    });

            // start over
            Socket.ReceiveAsync();
        }

        private void RaiseControlCommandReceivedEvent(IControlCommand command)
        {
            _logger.Trace("Raising ControlCommandReceivedEvent for {0}, {1}", command.DataType, command.Action);

            var handlers = ControlCommandReceived;
            if (handlers != null)
            {
                // raise event
                handlers(this, new ControlCommandReceivedEventArgs(command));
            }
        }

#if WINDOWS_PHONE

        /// <summary>
        /// This method is not supported on the phone and throws a <c>NotSupportedException</c> when it's used.
        /// </summary>
        public void Send(IControlCommand command)
        {
            // we need to implement this method on the phone (to comply with the interface)
            // but of course the phone cannot send control commands.
            throw new NotSupportedException("Send is not supported on the phone.");
        }

#endif

    }
}
