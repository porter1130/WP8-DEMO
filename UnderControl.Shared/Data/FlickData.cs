using System;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents a flick gestures.
    /// </summary>
    public sealed class FlickData : DataMessage
    {
        /// <summary>
        /// Gets or sets the timestamp the gesture was reported at.
        /// </summary>
        /// <value>
        /// The timestamp the gesture was reported at.
        /// </value>
        public DateTimeOffset Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the delta linked to this flick.
        /// Allows to determine the speed the gesture happened at.
        /// </summary>
        /// <value>
        /// The delta linked to this flick gesture.
        /// </value>
        public Vector2 Delta
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>Flick</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Flick;
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
            writer.Write(Timestamp.Ticks);
            writer.Write(Timestamp.Offset.Ticks);
            writer.Write(Delta.X);
            writer.Write(Delta.Y);
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
            var dateTimeTicks = reader.ReadInt64();
            var offsetTicks = reader.ReadInt64();
            var offset = TimeSpan.FromTicks(offsetTicks);
            Timestamp = new DateTimeOffset(dateTimeTicks, offset);
            Delta = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        #endregion
    }
}
