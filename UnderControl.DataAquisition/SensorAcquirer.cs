using System;
using Microsoft.Devices.Sensors;
using UnderControl.Shared.Data;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// A generic base class that implements the common features of all sensor acquirers.
    /// </summary>
    /// <typeparam name="T">The sensor type to use, must be of type <c>SensorBase&lt;TSensorReading&gt;</c>.</typeparam>
    /// <typeparam name="TSensorReading">The type of the sensor reading, must implement <c>ISensorReading</c>. It's the responsibility of
    /// the derived class to choose a suitable sensor reading type for the given <c>SensorBase</c> implementation.</typeparam>
    internal abstract class SensorAcquirer<T, TSensorReading>
        where T : SensorBase<TSensorReading>, new()
        where TSensorReading : ISensorReading
    {
        // it is not only safe to use a static field in a generic type here; this is in fact
        // the exact behavior we are looking for: to have a separate static logger for each
        // typed derived class.
        protected static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly T _sensor;
        private int _minDataRate;
        private readonly Action<DataMessage> _dataAcquiredCallback;
        private readonly string _typeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorAcquirer&lt;T, TSensorReading&gt;"/> class.
        /// </summary>
        /// <param name="dataAcquiredCallback">The data acquired callback that should be invoked when new data is available.</param>
        protected SensorAcquirer(Action<DataMessage> dataAcquiredCallback)
        {
            _dataAcquiredCallback = dataAcquiredCallback;
            _typeName = typeof(T).Name;

            // virtual member call in constructor
            // => we know it's safe to call because all 
            // derived classes only access static properties of the 
            // sensor classes
            if (IsSupported)
            {
                _sensor = new T();
                _sensor.CurrentValueChanged += Sensor_CurrentValueChanged;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type of sensor is supported.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this sensor type is supported; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsSupported
        {
            get;
        }

        /// <summary>
        /// Gets or sets the minimum data rate, in milliseconds.
        /// </summary>
        /// <value>
        /// The minimum data rate, in milliseconds.
        /// </value>
        public int MinDataRate
        {
            get
            {
                return _minDataRate;
            }
            set
            {
                if (_minDataRate != value)
                {
                    _minDataRate = value;
                    if (_sensor != null)
                    {
                        _sensor.TimeBetweenUpdates = TimeSpan.FromMilliseconds(_minDataRate);
                    }
                }
            }
        }

        /// <summary>
        /// Starts acquisition of data for this sensor, if the sensor is supported by the device.
        /// </summary>
        public void Start()
        {
            if (!IsSupported)
            {
                return;
            }

            _logger.Trace("Starting data acquisition: " + _typeName);

            _sensor.Start();
        }

        /// <summary>
        /// Stops acquisition of data for this sensor, if the sensor is supported by the device.
        /// </summary>
        public void Stop()
        {
            if (!IsSupported)
            {
                return;
            }

            _logger.Trace("Stopping data acquisition: " + _typeName);

            _sensor.Stop();
        }

        private void Sensor_CurrentValueChanged(object sender, SensorReadingEventArgs<TSensorReading> e)
        {
            _logger.Trace("Reporting data reading: {0}", _typeName);

            // get the reading and create new data message
            var reading = e.SensorReading;
            var data = CreateDataMessage(reading);

            _dataAcquiredCallback(data);
        }

        /// <summary>
        /// Creates a data message from a given reading.
        /// </summary>
        /// <param name="reading">The reading to use.</param>
        /// <returns>A data message of the correct type that contains the data from the reading.</returns>
        protected abstract DataMessage CreateDataMessage(TSensorReading reading);
    }
}
