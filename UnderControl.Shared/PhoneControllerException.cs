using System;

namespace UnderControl.Shared
{
    /// <summary>
    /// The custom application exception type for the UnderControl library.
    /// Apart from fundamental exception types like <c>ArgumentException</c>,
    /// <c>ArgumentNullException</c> and <c>InvalidOperationException</c>,
    /// all other exceptions in the library are wrapped in this type or derived types to simplify
    /// exception handling for client applications.
    /// </summary>
    public class PhoneControllerException : Exception
    {
        /// <summary>
        /// Gets or sets the error code, if applicable.
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
        /// Initializes a new instance of the <see cref="PhoneControllerException"/> class.
        /// </summary>
        public PhoneControllerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public PhoneControllerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errorCode">The error code to use.</param>
        public PhoneControllerException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public PhoneControllerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="errorCode">The error code to use.</param>
        public PhoneControllerException(string message, Exception innerException, string errorCode)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
