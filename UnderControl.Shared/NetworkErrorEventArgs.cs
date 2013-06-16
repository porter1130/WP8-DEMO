using System;

namespace UnderControl.Shared
{
    /// <summary>
    /// Event arguments that transport information about network errors.
    /// </summary>
    public class NetworkErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the underlying exception of the error, if applicable.
        /// </summary>
        /// <value>
        /// The exception object or <c>null</c> if no exception happened.
        /// </value>
        public Exception Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error code, for example a socket error, if applicable.
        /// </summary>
        /// <value>
        /// The error code or <c>null</c> if no error code is available.
        /// </value>
        public string ErrorCode
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkErrorEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message to use.</param>
        /// <param name="errorCode">The error code to use.</param>
        /// <param name="error">The underlying exception, if available.</param>
        public NetworkErrorEventArgs(string message, string errorCode, Exception error)
        {
            Message = message;
            ErrorCode = errorCode;
            Error = error;
        }
    }
}
