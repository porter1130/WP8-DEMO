using System;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents a pinch gesture.
    /// </summary>
    public sealed class PinchData : DataMessage
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
        /// Gets or sets the first touch point of the gesture.
        /// </summary>
        /// <value>
        /// The first touch point.
        /// </value>
        public TouchPoint TouchPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the second touch point of the gesture.
        /// </summary>
        /// <value>
        /// The second touch point.
        /// </value>
        public TouchPoint TouchPoint2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the first delta value of the gesture.
        /// </summary>
        /// <value>
        /// The first delta value.
        /// </value>
        public Vector2 Delta
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the second delta value of the gesture.
        /// </summary>
        /// <value>
        /// The second delta value.
        /// </value>
        public Vector2 Delta2
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>Pinch</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Pinch;
            }
        }

        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        public override bool MustBeDelivered
        {
            get
            {
                return false;
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
            writer.Write(TouchPoint.Id);
            writer.Write(TouchPoint.Location.X);
            writer.Write(TouchPoint.Location.Y);
            writer.Write((int)TouchPoint.State);
            writer.Write(TouchPoint2.Id);
            writer.Write(TouchPoint2.Location.X);
            writer.Write(TouchPoint2.Location.Y);
            writer.Write((int)TouchPoint2.State);
            writer.Write(Delta.X);
            writer.Write(Delta.Y);
            writer.Write(Delta2.X);
            writer.Write(Delta2.Y);
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
            TouchPoint2 = new TouchPoint()
            {
                Id = reader.ReadInt32(),
                Location = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                State = (TouchPointState)reader.ReadInt32()
            };
            Delta = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Delta2 = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        #endregion
    }
}
