using System;
using UnderControl.Shared.ControlCommands;

namespace UnderControl.Shared.Channels
{
    /// <summary>
    /// Event arguments for <c>ControlCommandReceived</c> events.
    /// </summary>
    public class ControlCommandReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the control command that was received.
        /// </summary>
        public IControlCommand ControlCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlCommandReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="controlCommand">The control command that was received.</param>
        public ControlCommandReceivedEventArgs(IControlCommand controlCommand)
        {
            ControlCommand = controlCommand;
        }
    }
}
