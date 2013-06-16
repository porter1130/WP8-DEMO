using System;
using UnderControl.Shared.ControlCommands;

namespace UnderControl.Shared.Channels
{
    /// <summary>
    /// An interface that describes a specialized channel used to transmit control commands from the server to the client.
    /// </summary>
    public interface IControlChannel : IChannel
    {
        /// <summary>
        /// Occurs when a control command was successfully sent from the server to the client, on the server side.
        /// </summary>
        event EventHandler<EventArgs> ControlCommandSent;

        /// <summary>
        /// Occurs when a control command was successfully received, on the client side.
        /// </summary>
        event EventHandler<ControlCommandReceivedEventArgs> ControlCommandReceived;

        /// <summary>
        /// Sends the specified command to the client.
        /// </summary>
        /// <param name="command">The command to send.</param>
        void Send(IControlCommand command);
    }
}