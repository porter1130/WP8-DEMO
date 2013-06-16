using System;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Event arguments to transport a <c>DataMessage</c>.
    /// </summary>
    public class DataMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the data message contained in the event arguments.
        /// </summary>
        public IDataMessage DataMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMessageEventArgs"/> class.
        /// </summary>
        /// <param name="dataMessage">The data message.</param>
        public DataMessageEventArgs(IDataMessage dataMessage)
        {
            DataMessage = dataMessage;
        }
    }
}