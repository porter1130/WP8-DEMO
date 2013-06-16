using System;
using Microsoft.Devices.Sensors;
using UnderControl.Shared.Data;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// A specific sensor acquirer implementation for the accelerometer sensor.
    /// </summary>
    internal sealed class AccelerometerAcquirer : SensorAcquirer<Accelerometer, AccelerometerReading>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccelerometerAcquirer"/> class.
        /// </summary>
        /// <param name="dataAcquiredCallback">The data acquired callback that is invoked when a new reading is available.</param>
        public AccelerometerAcquirer(Action<DataMessage> dataAcquiredCallback)
            : base(dataAcquiredCallback)
        {
        }

        #region Overrides of SensorAcquirer<Accelerometer,AccelerometerReading>

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
                return Accelerometer.IsSupported;
            }
        }

        /// <summary>
        /// Creates a data message from a given reading.
        /// </summary>
        /// <param name="reading">The reading to use.</param>
        /// <returns>A data message of the correct type that contains the data from the reading.</returns>
        protected override DataMessage CreateDataMessage(AccelerometerReading reading)
        {
            var data = new AccelerometerData
            {
                X = reading.Acceleration.X,
                Y = reading.Acceleration.Y,
                Z = reading.Acceleration.Z,
                Timestamp = reading.Timestamp
            };

            return data;
        }

        #endregion
    }
}
