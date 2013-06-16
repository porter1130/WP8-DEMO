using System;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents a tap gesture.
    /// </summary>
    public class TapData : DataMessage
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
        /// Gets or sets the touch point the gesture happened at.
        /// </summary>
        /// <value>
        /// The touch point the gesture happened at.
        /// </value>
        public TouchPoint TouchPoint
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>Tap</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Tap;
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

        /// <summary>Adds custom data to the raw representation of the data message.
        /// </summary>
        /// <param name="writer">The binary writer used to create the raw representation of the data message.</param>
        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(Timestamp.Ticks);
            writer.Write(Timestamp.Offset.Ticks);
            writer.Write(TouchPoint.Id);
            writer.Write(TouchPoint.Location.X);
            writer.Write(TouchPoint.Location.Y);
            writer.Write((int)TouchPoint.State);
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
            TouchPoint = new TouchPoint()
                           {
                               Id = reader.ReadInt32(),
                               Location = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                               State = (TouchPointState)reader.ReadInt32()
                           };
        }

        #endregion
    }
}
