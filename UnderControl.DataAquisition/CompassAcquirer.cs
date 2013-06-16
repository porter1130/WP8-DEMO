using System;
using Microsoft.Devices.Sensors;
using UnderControl.Shared.Data;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// A specific sensor acquirer implementation for the compass sensor.
    /// </summary>
    internal sealed class CompassAcquirer : SensorAcquirer<Compass, CompassReading>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompassAcquirer"/> class.
        /// </summary>
        /// <param name="dataAcquiredCallback">The data acquired callback that is invoked when a new reading is available.</param>
        public CompassAcquirer(Action<DataMessage> dataAcquiredCallback)
            : base(dataAcquiredCallback)
        {
        }

        #region Overrides of SensorAcquirer<Compass,CompassReading>

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
                return Compass.IsSupported;
            }
        }

        /// <summary>
        /// Creates a data message from a given reading.
        /// </summary>
        /// <param name="reading">The reading to use.</param>
        /// <returns>A data message of the correct type that contains the data from the reading.</returns>
        protected override DataMessage CreateDataMessage(CompassReading reading)
        {
            var data = new CompassData();

            data.HeadingAccuracy = reading.HeadingAccuracy;
            data.MagneticHeading = reading.MagneticHeading;
            data.MagnetometerReading = new Vector3(reading.MagnetometerReading.X,
                reading.MagnetometerReading.Y,
                reading.MagnetometerReading.Z);
            data.Timestamp = reading.Timestamp;
            data.TrueHeading = reading.TrueHeading;

            return data;
        }

        #endregion
    }
}
