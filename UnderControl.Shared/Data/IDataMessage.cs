namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Describes a piece of data acquired on the phone and sent to a remote server.
    /// </summary>
    public interface IDataMessage
    {
        /// <summary>
        /// Gets or sets the total length of the data message, in bytes.
        /// </summary>
        /// <value>
        /// The length of the data message in bytes.
        /// </value>
        byte Length
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the type of data contained in the data message.
        /// </summary>
        /// <value>
        /// The type of data contained in the data message.
        /// </value>
        DataType DataType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the data message must be delivered reliably or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the data message must be delivered reliably; otherwise, <c>false</c>.
        /// </value>
        bool MustBeDelivered
        {
            get;
        }

        /// <summary>
        /// Creates a raw representation of the data message.
        /// </summary>
        /// <returns>A byte array containing the raw representation of the data.</returns>
        byte[] ToByteArray();

        /// <summary>
        /// Recreates the data message content froms a raw byte array representation, starting at a given offset in the raw data.
        /// </summary>
        /// <param name="rawData">The raw data to use.</param>
        /// <param name="start">The starting index to restore the data message content from.</param>
        void FromByteArray(byte[] rawData, int start);
    }
}