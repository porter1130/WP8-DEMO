using System;

namespace UnderControl.Shared
{
    /// <summary>
    /// Event arguments that transport information about state changes of a controller.
    /// </summary>
    public class PhoneControllerStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new state of the controller.
        /// </summary>
        public PhoneControllerState State
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerStateEventArgs"/> class.
        /// </summary>
        /// <param name="state">The new state of the controller.</param>
        public PhoneControllerStateEventArgs(PhoneControllerState state)
        {
            State = state;
        }
    }
}
