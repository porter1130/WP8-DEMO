using System;

namespace UnderControl.Shared.Sockets
{
    /// <summary>
    /// Event arguments used to transport received raw data.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the received raw data.
        /// </summary>
        /// <value>
        /// The received raw data.
        /// </value>
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="data">The received raw data.</param>
        public DataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}