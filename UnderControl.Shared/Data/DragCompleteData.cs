
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Represents a data message that signals completion of any of the following drag operations:
    /// <c>FreeDrag</c>, <c>HorizontalDrag</c> and <c>VerticalDrag</c>.
    /// </summary>
    /// <remarks>Completion of the <c>CustomDrag</c> operation is reported by a different data message, in particular <c>CustomDragComplete</c>.</remarks>
    public sealed class DragCompleteData : DataMessage
    {
        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>DragComplete</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.DragComplete;
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
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
        }

        #endregion
    }
}
