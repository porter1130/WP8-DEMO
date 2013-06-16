using System;
using Microsoft.Devices.Sensors;
using UnderControl.Shared.Data;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// A specific sensor acquirer implementation for the combined motion API.
    /// </summary>
    internal sealed class MotionAcquirer : SensorAcquirer<Motion, MotionReading>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MotionAcquirer"/> class.
        /// </summary>
        /// <param name="dataAcquiredCallback">The data acquired callback that is invoked when a new reading is available.</param>
        public MotionAcquirer(Action<DataMessage> dataAcquiredCallback)
            : base(dataAcquiredCallback)
        {
        }

        #region Overrides of SensorAcquirer<Motion,MotionReading>

        /// <summary>
        /// Gets a value indicating whether this type of sensor is supported.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this sensor type is supported; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSupported
        {
            get
            {
                return Motion.IsSupported;
            }
        }

        /// <summary>
        /// Creates a data message from a given reading.
        /// </summary>
        /// <param name="reading">The reading to use.</param>
        /// <returns>A data message of the correct type that contains the data from the reading.</returns>
        protected override DataMessage CreateDataMessage(MotionReading reading)
        {
            var data = new MotionData();

            // attitude
            var readingAttitude = reading.Attitude;
            var readingAttitudeQuaternion = reading.Attitude.Quaternion;
            var readingAttitudeRotationMatrix = reading.Attitude.RotationMatrix;
            var attitude = new Shared.Data.AttitudeReading
                               {
                                   Yaw = readingAttitude.Yaw,
                                   Pitch = readingAttitude.Pitch,
                                   Roll = readingAttitude.Roll,
                                   Quaternion = new Quaternion(readingAttitudeQuaternion.X,
                                                               readingAttitudeQuaternion.Y,
                                                               readingAttitudeQuaternion.Z,
                                                               readingAttitudeQuaternion.W),
                                   RotationMatrix = new Matrix
                                                        {
                                                            M11 = readingAttitudeRotationMatrix.M11,
                                                            M12 = readingAttitudeRotationMatrix.M12,
                                                            M13 = readingAttitudeRotationMatrix.M13,
                                                            M14 = readingAttitudeRotationMatrix.M14,
                                                            M21 = readingAttitudeRotationMatrix.M21,
                                                            M22 = readingAttitudeRotationMatrix.M22,
                                                            M23 = readingAttitudeRotationMatrix.M23,
                                                            M24 = readingAttitudeRotationMatrix.M24,
                                                            M31 = readingAttitudeRotationMatrix.M31,
                                                            M32 = readingAttitudeRotationMatrix.M32,
                                                            M33 = readingAttitudeRotationMatrix.M33,
                                                            M34 = readingAttitudeRotationMatrix.M34,
                                                            M41 = readingAttitudeRotationMatrix.M41,
                                                            M42 = readingAttitudeRotationMatrix.M42,
                                                            M43 = readingAttitudeRotationMatrix.M43,
                                                            M44 = readingAttitudeRotationMatrix.M44
                                                        },
                                   TimeStamp = readingAttitude.Timestamp
                               };
            data.Attitude = attitude;

            // other data
            data.DeviceAcceleration = new Vector3(reading.DeviceAcceleration.X,
                reading.DeviceAcceleration.Y,
                reading.DeviceAcceleration.Z);

            data.DeviceRotationRate = new Vector3(reading.DeviceRotationRate.X,
                reading.DeviceRotationRate.Y,
                reading.DeviceRotationRate.Z);

            data.Gravity = new Vector3(reading.Gravity.X,
                reading.Gravity.Y,
                reading.Gravity.Z);

            data.Timestamp = reading.Timestamp;

            return data;
        }

        #endregion
    }
}
