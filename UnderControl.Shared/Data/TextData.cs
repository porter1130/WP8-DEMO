using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents text input.
    /// </summary>
    public class TextData : DataMessage
    {
        /// <summary>
        /// Gets or sets the text or text fragment that was input.
        /// If a text is longer than the maximum allowed length of a data message, it will be fragmented into 
        /// several individual <c>TextData</c> messages.
        /// </summary>
        /// <value>
        /// The text or text fragment.
        /// </value>
        public string Text
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>Text</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Text;
            }
        }

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public override bool MustBeDelivered
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Adds custom data to the raw representation of the data message.
        /// </summary>
        /// <param name="writer">The binary writer used to create the raw representation of the data message.</param>
        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(Text);
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
            Text = reader.ReadString();
        }

        #endregion
    }
}
