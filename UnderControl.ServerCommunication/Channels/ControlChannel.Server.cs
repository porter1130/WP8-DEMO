using System;
using System.Collections.Generic;
using System.Net;
using UnderControl.Shared.Channels;
using UnderControl.Shared.ControlCommands;

namespace UnderControl.Communication.Channels
{
    internal sealed partial class ControlChannel : IControlChannel
    {
        private readonly Queue<IControlCommand> _commandQueue = new Queue<IControlCommand>();
        private bool _isSending;

        protected override void OnListenAsync(IPEndPoint localEndPoint)
        {
            _logger.Trace("Extending base class initialization");

            Socket.DataSent += Socket_DataSent;
        }

        /// <summary>
        /// Sends the specified command to the client.
        /// </summary>
        /// <param name="command">The command to send.</param>
        public void Send(IControlCommand command)
        {
            Guard();

            _logger.Trace("Send request for control command {0}, {1}", command.DataType, command.Action);

            lock (_commandQueue)
            {
                if (_isSending)
                {
                    _commandQueue.Enqueue(command);
                }
                else
                {
                    SendControlCommand(command);
                }
            }
        }

        private void SendControlCommand(IControlCommand command)
        {
            _logger.Trace("Sending control command {0}, {1}", command.DataType, command.Action);

            _isSending = true;

            // send
            var rawCommand = command.ToByteArray();
            Socket.SendAsync(rawCommand);
        }

        private void Guard()
        {
            if (Socket == null || !Socket.IsConnected)
            {
                throw new InvalidOperationException("Cannot send control command without being connected.");
            }
        }

        private void Socket_DataSent(object sender, EventArgs e)
        {
            _logger.Trace("Received DataSent event from socket");

            // check if we have more commands to send
            lock (_commandQueue)
            {
                _isSending = false;

                if (_commandQueue.Count > 0)
                {
                    var nextCommand = _commandQueue.Dequeue();
                    SendControlCommand(nextCommand);
                }
            }

            _logger.Trace("Raising event ControlCommandSent");

            // raise event
            var handlers = ControlCommandSent;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        partial void OnCleaningUpPartial()
        {
            _logger.Trace("Partial method clean up");

            if (Socket != null)
            {
                Socket.DataSent -= Socket_DataSent;
            }

            // clean up queue
            lock (_commandQueue)
            {
                _isSending = false;
                _commandQueue.Clear();
            }
        }
    }
}
