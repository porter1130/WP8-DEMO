using System;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A base class for all drag data messages.
    /// </summary>
    public abstract class DragDataBase : DataMessage
    {
        /// <summary>
        /// Gets or sets the timestamp the data was acquired at.
        /// </summary>
        public DateTimeOffset Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the touch point of the drag data.
        /// </summary>
        public TouchPoint TouchPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the delta to the last reported drag touch point.
        /// </summary>
        public Vector2 Delta
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Gets a value indicating whether the data message must be delivered reliably or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the data message must be delivered reliably; otherwise, <c>false</c>.
        /// </value>
        public override bool MustBeDelivered
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Adds custom drag data to the raw representation of the data message.
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
            writer.Write(Delta.X);
            writer.Write(Delta.Y);
        }

        /// <summary>
        /// Reads back custom drag data from a raw representation of the data message.
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
            Delta = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        #endregion
    }
}
