using System;
using Microsoft.Devices.Sensors;
using UnderControl.Shared.Data;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// A specific sensor acquirer implementation for the gyroscope sensor.
    /// </summary>
    internal sealed class GyroscopeAcquirer : SensorAcquirer<Gyroscope, GyroscopeReading>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GyroscopeAcquirer"/> class.
        /// </summary>
        /// <param name="dataAcquiredCallback">The data acquired callback that is invoked when a new reading is available.</param>
        public GyroscopeAcquirer(Action<DataMessage> dataAcquiredCallback)
            : base(dataAcquiredCallback)
        {
        }

        #region Overrides of SensorAcquirer<Gyroscope,GyroscopeReading>

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
                return Gyroscope.IsSupported;
            }
        }

        /// <summary>
        /// Creates a data message from a given reading.
        /// </summary>
        /// <param name="reading">The reading to use.</param>
        /// <returns>A data message of the correct type that contains the data from the reading.</returns>
        protected override DataMessage CreateDataMessage(GyroscopeReading reading)
        {
            var data = new GyroscopeData();

            data.RotationRate = new Vector3(reading.RotationRate.X,
                reading.RotationRate.Y,
                reading.RotationRate.Z);
            data.Timestamp = reading.Timestamp;

            return data;
        }

        #endregion
    }
}
