
using System;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents a reading from the combined motion API.
    /// </summary>
    public sealed class MotionData : DataMessage
    {
        /// <summary>
        /// Gets or sets the attitude of the reading.
        /// </summary>
        /// <value>
        /// The attitude of the reading.
        /// </value>
        public AttitudeReading Attitude
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device acceleration of the reading.
        /// </summary>
        /// <value>
        /// The device acceleration of the reading.
        /// </value>
        public Vector3 DeviceAcceleration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device rotation rate of the reading.
        /// </summary>
        /// <value>
        /// The device rotation rate of the reading.
        /// </value>
        public Vector3 DeviceRotationRate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the gravity of the reading.
        /// </summary>
        /// <value>
        /// The gravity of the reading.
        /// </value>
        public Vector3 Gravity
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
        /// Returns data type <c>Motion</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Motion;
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
            // attitude
            writer.Write(Attitude.Yaw);
            writer.Write(Attitude.Pitch);
            writer.Write(Attitude.Roll);
            writer.Write(Attitude.Quaternion.X);
            writer.Write(Attitude.Quaternion.Y);
            writer.Write(Attitude.Quaternion.Z);
            writer.Write(Attitude.Quaternion.W);
            writer.Write(Attitude.RotationMatrix.M11);
            writer.Write(Attitude.RotationMatrix.M12);
            writer.Write(Attitude.RotationMatrix.M13);
            writer.Write(Attitude.RotationMatrix.M14);
            writer.Write(Attitude.RotationMatrix.M21);
            writer.Write(Attitude.RotationMatrix.M22);
            writer.Write(Attitude.RotationMatrix.M23);
            writer.Write(Attitude.RotationMatrix.M24);
            writer.Write(Attitude.RotationMatrix.M31);
            writer.Write(Attitude.RotationMatrix.M32);
            writer.Write(Attitude.RotationMatrix.M33);
            writer.Write(Attitude.RotationMatrix.M34);
            writer.Write(Attitude.RotationMatrix.M41);
            writer.Write(Attitude.RotationMatrix.M42);
            writer.Write(Attitude.RotationMatrix.M43);
            writer.Write(Attitude.RotationMatrix.M44);
            writer.Write(Attitude.TimeStamp.Ticks);
            writer.Write(Attitude.TimeStamp.Offset.Ticks);

            // acceleration
            writer.Write(DeviceAcceleration.X);
            writer.Write(DeviceAcceleration.Y);
            writer.Write(DeviceAcceleration.Z);

            // rotation rate
            writer.Write(DeviceRotationRate.X);
            writer.Write(DeviceRotationRate.Y);
            writer.Write(DeviceRotationRate.Z);

            // gravity
            writer.Write(Gravity.X);
            writer.Write(Gravity.Y);
            writer.Write(Gravity.Z);

            // timestamp
            writer.Write(Timestamp.Ticks);
            writer.Write(Timestamp.Offset.Ticks);
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
            // attitude
            Attitude.Yaw = reader.ReadSingle();
            Attitude.Pitch = reader.ReadSingle();
            Attitude.Roll = reader.ReadSingle();
            Attitude.Quaternion = new Quaternion();
            Attitude.Quaternion.X = reader.ReadSingle();
            Attitude.Quaternion.Y = reader.ReadSingle();
            Attitude.Quaternion.Z = reader.ReadSingle();
            Attitude.Quaternion.W = reader.ReadSingle();
            Attitude.RotationMatrix = new Matrix();
            Attitude.RotationMatrix.M11 = reader.ReadSingle();
            Attitude.RotationMatrix.M12 = reader.ReadSingle();
            Attitude.RotationMatrix.M13 = reader.ReadSingle();
            Attitude.RotationMatrix.M14 = reader.ReadSingle();
            Attitude.RotationMatrix.M21 = reader.ReadSingle();
            Attitude.RotationMatrix.M22 = reader.ReadSingle();
            Attitude.RotationMatrix.M23 = reader.ReadSingle();
            Attitude.RotationMatrix.M24 = reader.ReadSingle();
            Attitude.RotationMatrix.M31 = reader.ReadSingle();
            Attitude.RotationMatrix.M32 = reader.ReadSingle();
            Attitude.RotationMatrix.M33 = reader.ReadSingle();
            Attitude.RotationMatrix.M34 = reader.ReadSingle();
            Attitude.RotationMatrix.M41 = reader.ReadSingle();
            Attitude.RotationMatrix.M42 = reader.ReadSingle();
            Attitude.RotationMatrix.M43 = reader.ReadSingle();
            Attitude.RotationMatrix.M44 = reader.ReadSingle();
            var dateTimeTicks = reader.ReadInt64();
            var offsetTicks = reader.ReadInt64();
            var offset = TimeSpan.FromTicks(offsetTicks);
            Attitude.TimeStamp = new DateTimeOffset(dateTimeTicks, offset);

            // acceleration
            DeviceAcceleration.X = reader.ReadSingle();
            DeviceAcceleration.Y = reader.ReadSingle();
            DeviceAcceleration.Z = reader.ReadSingle();

            // rotation rate
            DeviceRotationRate.X = reader.ReadSingle();
            DeviceRotationRate.Y = reader.ReadSingle();
            DeviceRotationRate.Z = reader.ReadSingle();

            // gravity
            Gravity.X = reader.ReadSingle();
            Gravity.Y = reader.ReadSingle();
            Gravity.Z = reader.ReadSingle();

            // timestamp
            dateTimeTicks = reader.ReadInt64();
            offsetTicks = reader.ReadInt64();
            offset = TimeSpan.FromTicks(offsetTicks);
            Timestamp = new DateTimeOffset(dateTimeTicks, offset);
        }

        #endregion
    }
}
