
using System;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents compass data.
    /// </summary>
    public sealed class CompassData : DataMessage
    {
        /// <summary>
        /// Gets or sets the heading accuracy.
        /// </summary>
        /// <value>
        /// The heading accuracy.
        /// </value>
        public double HeadingAccuracy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the magnetic heading.
        /// </summary>
        /// <value>
        /// The magnetic heading.
        /// </value>
        public double MagneticHeading
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the magnetometer reading.
        /// </summary>
        /// <value>
        /// The magnetometer reading.
        /// </value>
        public Vector3 MagnetometerReading
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timestamp of the reading.
        /// </summary>
        /// <value>
        /// The timestamp of the reading.
        /// </value>
        public DateTimeOffset Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the true heading.
        /// </summary>
        /// <value>
        /// The true heading.
        /// </value>
        public double TrueHeading
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>Compass</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Compass;
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
            writer.Write(HeadingAccuracy);
            writer.Write(MagneticHeading);
            writer.Write(MagnetometerReading.X);
            writer.Write(MagnetometerReading.Y);
            writer.Write(MagnetometerReading.Z);
            writer.Write(Timestamp.Ticks);
            writer.Write(Timestamp.Offset.Ticks);
            writer.Write(TrueHeading);
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
            HeadingAccuracy = reader.ReadDouble();
            MagneticHeading = reader.ReadDouble();
            MagnetometerReading = new Vector3();
            MagnetometerReading.X = reader.ReadSingle();
            MagnetometerReading.Y = reader.ReadSingle();
            MagnetometerReading.Z = reader.ReadSingle();
            var dateTimeTicks = reader.ReadInt64();
            var offsetTicks = reader.ReadInt64();
            var offset = TimeSpan.FromTicks(offsetTicks);
            Timestamp = new DateTimeOffset(dateTimeTicks, offset);
            TrueHeading = reader.ReadDouble();
        }

        #endregion
    }
}
