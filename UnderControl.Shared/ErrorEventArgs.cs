using System;

namespace UnderControl.Shared
{
    /// <summary>
    /// Event arguments used to transport error information.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the error as <c>PhoneControllerException</c> instance.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public PhoneControllerException Error
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="error">The error to use.</param>
        public ErrorEventArgs(PhoneControllerException error)
        {
            Error = error;
        }
    }
}
