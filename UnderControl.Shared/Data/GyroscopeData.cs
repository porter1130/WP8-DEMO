
using System;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents a gyroscope reading.
    /// </summary>
    public sealed class GyroscopeData : DataMessage
    {
        /// <summary>
        /// Gets or sets the rotation rate of the reading.
        /// </summary>
        /// <value>
        /// The rotation rate of the reading.
        /// </value>
        public Vector3 RotationRate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timestamp the reading was reported at.
        /// </summary>
        /// <value>
        /// The timestamp the reading was reported at.
        /// </value>
        public DateTimeOffset Timestamp
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>Gyroscope</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Gyroscope;
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
            writer.Write(RotationRate.X);
            writer.Write(RotationRate.Y);
            writer.Write(RotationRate.Z);
            writer.Write(Timestamp.Ticks);
            writer.Write(Timestamp.Offset.Ticks);
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
            RotationRate = new Vector3();
            RotationRate.X = reader.ReadSingle();
            RotationRate.Y = reader.ReadSingle();
            RotationRate.Z = reader.ReadSingle();
            var dateTimeTicks = reader.ReadInt64();
            var offsetTicks = reader.ReadInt64();
            var offset = TimeSpan.FromTicks(offsetTicks);
            Timestamp = new DateTimeOffset(dateTimeTicks, offset);
        }

        #endregion
    }
}
