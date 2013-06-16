using System;
using System.IO;
using System.Text;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// An abstract base class that contains fundamental functionality shared by all data message types.
    /// </summary>
    public abstract class DataMessage : IDataMessage
    {
        /// <summary>
        /// The maximum length, in bytes, of a data message.
        /// </summary>
        public const int MaxLength = byte.MaxValue;

        /// <summary>
        /// The maximum payload length, in bytes, available to derived data message classes after considering all meta data.
        /// </summary>
        public const int MaxPayloadLength = (MaxLength - sizeof(byte) - sizeof(Int32)); // one byte for the length, one Int32 for the data type

        /// <summary>
        /// Gets or sets the total length of the data message, in bytes.
        /// </summary>
        /// <value>
        /// The length of the data message in bytes.
        /// </value>
        public byte Length
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
        public abstract DataType DataType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the data message must be delivered reliably or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the data message must be delivered reliably; otherwise, <c>false</c>.
        /// </value>
        public abstract bool MustBeDelivered
        {
            get;
        }

        /// <summary>
        /// Creates a raw representation of the data message.
        /// </summary>
        /// <returns>
        /// A byte array containing the raw representation of the data.
        /// </returns>
        public byte[] ToByteArray()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms, Encoding.UTF8);

            // write the length placeholder
            bw.Write((byte)0);
            bw.Write((Int32)DataType);

            // let the derived class write its content
            WriteData(bw);

            bw.Dispose();

            var result = ms.ToArray();

            if (result.Length > byte.MaxValue)
            {
                throw new InvalidOperationException("Data message length is too big!");
            }

            result[0] = (byte)result.Length;
            return result;
        }

        /// <summary>
        /// Recreates the data message content froms a raw byte array representation, starting at a given offset in the raw data.
        /// </summary>
        /// <param name="rawData">The raw data to use.</param>
        /// <param name="start">The starting index to restore the data message content from.</param>
        public void FromByteArray(byte[] rawData, int start)
        {
            var ms = new MemoryStream(rawData, start, rawData.Length - start);
            var br = new BinaryReader(ms, Encoding.UTF8);

            // get length
            Length = br.ReadByte();

            // make sure this is correct
            var dataType = (DataType)br.ReadInt32();
            if (dataType != DataType)
            {
                throw new InvalidOperationException("Message type " + DataType + " cannot read raw data for message type " + dataType);
            }

            // let the derived class do the work
            ReadData(br);

            br.Dispose();
        }

        /// <summary>
        /// When overwritten in a derived class, adds custom data to the raw representation of the data message.
        /// </summary>
        /// <param name="writer">The binary writer used to create the raw representation of the data message.</param>
        protected abstract void WriteData(BinaryWriter writer);

        /// <summary>
        /// When overwritten in a derived class, reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected abstract void ReadData(BinaryReader reader);
    }
}
