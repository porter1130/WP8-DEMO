
using System;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents accelerometer data.
    /// </summary>
    public sealed class AccelerometerData : DataMessage
    {
        /// <summary>
        /// Gets or sets the X coordinate of the acceleration.
        /// </summary>
        public float X
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Y coordinate of the acceleration.
        /// </summary>
        public float Y
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Z coordinate of the acceleration.
        /// </summary>
        public float Z
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timestamp of the data reading.
        /// </summary>
        public DateTimeOffset Timestamp
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns the data type <c>Accelerometer</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Accelerometer;
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
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(Timestamp.Ticks);
            writer.Write(Timestamp.Offset.Ticks);
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            var dateTimeTicks = reader.ReadInt64();
            var offsetTicks = reader.ReadInt64();
            var offset = TimeSpan.FromTicks(offsetTicks);
            Timestamp = new DateTimeOffset(dateTimeTicks, offset);
        }

        #endregion
    }
}
