using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Devices.Sensors;
using UnderControl.Shared;
using UnderControl.Shared.Data;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// An implementation of the <c>IDataSource</c> interface that uses several Windows Phone specific implementations.
    /// </summary>
    public sealed class DataSource : IDataSource
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SynchronizationContext _synchronizationContext;
        private readonly object _dataAcquiredEventLocker = new object();
        private readonly object _locker = new object();

        private TouchAcquirer _touchAcquirer;
        private GestureAcquirer _gestureAcquirer;
        private AccelerometerAcquirer _accelerometerAcquirer;
        private CompassAcquirer _compassAcquirer;
        private GyroscopeAcquirer _gyroscopeAcquirer;
        private MotionAcquirer _motionAcquirer;

        private readonly List<DataType> _activeDataTypes = new List<DataType>();
        private readonly List<DataType> _pausedDataTypes = new List<DataType>();

        private TouchAcquirer TouchAcquirer
        {
            get
            {
                if (_touchAcquirer == null)
                {
                    _touchAcquirer = new TouchAcquirer(RaiseDataAcquiredEvent);
                }

                return _touchAcquirer;
            }
        }

        private GestureAcquirer GestureAcquirer
        {
            get
            {
                if (_gestureAcquirer == null)
                {
                    _gestureAcquirer = new GestureAcquirer(RaiseDataAcquiredEvent);
                }

                return _gestureAcquirer;
            }
        }

        private AccelerometerAcquirer AccelerometerAcquirer
        {
            get
            {
                if (_accelerometerAcquirer == null)
                {
                    _accelerometerAcquirer = new AccelerometerAcquirer(RaiseDataAcquiredEvent);
                }

                return _accelerometerAcquirer;
            }
        }

        private CompassAcquirer CompassAcquirer
        {
            get
            {
                if (_compassAcquirer == null)
                {
                    _compassAcquirer = new CompassAcquirer(RaiseDataAcquiredEvent);
                }

                return _compassAcquirer;
            }
        }

        private GyroscopeAcquirer GyroscopeAcquirer
        {
            get
            {
                if (_gyroscopeAcquirer == null)
                {
                    _gyroscopeAcquirer = new GyroscopeAcquirer(RaiseDataAcquiredEvent);
                }

                return _gyroscopeAcquirer;
            }
        }

        private MotionAcquirer MotionAcquirer
        {
            get
            {
                if (_motionAcquirer == null)
                {
                    _motionAcquirer = new MotionAcquirer(RaiseDataAcquiredEvent);
                }

                return _motionAcquirer;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSource"/> class.
        /// </summary>
        public DataSource()
        {
            // grab the current synchronization context
            _synchronizationContext = SynchronizationContext.Current;
            if (_synchronizationContext == null)
            {
                // create a default implementation
                _synchronizationContext = new SynchronizationContext();
            }
        }

        #region IDataSource

        /// <summary>
        /// Raised when new data has been acquired from any data acquirer.
        /// </summary>
        /// <remarks>
        /// Important: this event is not guaranteed to be raised on the synchronization context of the thread that created the data source.
        /// For performance reasons, no marshaling happens. If you intend to consume this event from a UI thread, you have to dispatch
        /// this event manually.
        /// </remarks>
        public event EventHandler<DataMessageEventArgs> DataAcquired;

        /// <summary>
        /// Raised when the types data is acquired for have changed. 
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be raised on the synchronization context of the thread that created the data source.
        /// </remarks>
        public event EventHandler<DataTypesChangedEventArgs> DataTypesChanged;

        private void RaiseDataAcquiredEvent(DataMessage message)
        {
            // check if this data type is paused or not active
            lock (_locker)
            {
                if (_pausedDataTypes.Contains(message.DataType))
                {
                    // do not report data that is currently paused
                    return;
                }

                if (!_activeDataTypes.Contains(message.DataType))
                {
                    // do not report data that is currently not active 
                    // (this case indicates a small overlap between data acquisition and a stop control message,
                    // or an inconsistency if it happens permanently - but by ignoring it we at least keep the 
                    // server in a consistent state and provide expected behavior).
                    return;
                }
            }

            _logger.Trace("Raising event DataAcquired for {0}", message.DataType);

            // this is not synchronized to the context of the creator (e.g UI thread) automatically;
            // however, what we do ensure is that no more than one of these events are raised
            // at the same time to allow easier/safer sequential processing
            lock (_dataAcquiredEventLocker)
            {
                var handlers = DataAcquired;
                if (handlers != null)
                {
                    handlers(this, new DataMessageEventArgs(message));
                }
            }
        }

        private void RaiseDataTypesChangedEvent()
        {
            _logger.Trace("Raising event DataTypesChanged");

            var activeTypes = GetActiveDataTypes();

            // make sure we only raise events on the correct context (relevant when data source is used on the UI thread)
            _synchronizationContext.Post(o =>
            {
                var handlers = DataTypesChanged;
                if (handlers != null)
                {
                    var args = new DataTypesChangedEventArgs(activeTypes);
                    handlers(this, args);
                }
            }, null);
        }

        /// <summary>
        /// Starts acquisition of a certain data type.
        /// </summary>
        /// <param name="dataType">The type of data to start acquisition for.</param>
        public void StartAcquisition(DataType dataType)
        {
            try
            {
                // synchronize so that start/stop/pause/resume do not overlap
                lock (_locker)
                {
                    if (_activeDataTypes.Contains(dataType))
                    {
                        return;
                    }

                    _logger.Trace("Starting acquisition of data type {0}", dataType);

                    switch (dataType)
                    {
                        case DataType.Accelerometer:
                            AccelerometerAcquirer.Start();
                            break;
                        case DataType.Compass:
                            CompassAcquirer.Start();
                            break;
                        case DataType.Gyroscope:
                            GyroscopeAcquirer.Start();
                            break;
                        case DataType.Motion:
                            MotionAcquirer.Start();
                            break;
                        case DataType.Touch:
                            TouchAcquirer.Start(dataType);
                            break;
                        case DataType.Tap:
                        case DataType.DoubleTap:
                        case DataType.Hold:
                        case DataType.Flick:
                        case DataType.FreeDrag:
                        case DataType.HorizontalDrag:
                        case DataType.VerticalDrag:
                        case DataType.DragComplete:
                        case DataType.Pinch:
                        case DataType.PinchComplete:
                            GestureAcquirer.Start(dataType);
                            break;
                        case DataType.CustomDrag:
                            TouchAcquirer.Start(dataType);
                            break;
                        case DataType.CustomDragComplete:
                            // this needs to be sent manually using the "AddData" method.
                            break;
                        case DataType.Text:
                            // we do not have an explicit text data source
                            // since this requires some sort of UI/user interaction with a text box etc.
                            // text is added from external sources using the "AddData" method.
                            // we do however add this to the active data types (below) to signal to the outside 
                            // that actions should be taken to enable the user to input text
                            break;
                        default:
                            // an unsupported data type?
                            if (Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }
                            return;
                    }

                    // add the data type to the list of active ones
                    _activeDataTypes.Add(dataType);
                }

                // raise event
                RaiseDataTypesChangedEvent();
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException(string.Format("Error while starting acquisition of data {0}: {1}", dataType, ex.Message), ex);
            }
        }

        /// <summary>
        /// Stops acquisition of a certain data type.
        /// </summary>
        /// <param name="dataType">The type of the data to stop acquisition for.</param>
        public void StopAcquisition(DataType dataType)
        {
            try
            {
                // synchronize so that start/stop do not overlap
                lock (_locker)
                {
                    if (!_activeDataTypes.Contains(dataType))
                    {
                        return;
                    }

                    _logger.Trace("Stopping acquisition of data type {0}", dataType);

                    // remove
                    _activeDataTypes.Remove(dataType);
                    if (_pausedDataTypes.Contains(dataType))
                    {
                        _pausedDataTypes.Remove(dataType);
                    }

                    switch (dataType)
                    {
                        case DataType.Accelerometer:
                            AccelerometerAcquirer.Stop();
                            break;
                        case DataType.Compass:
                            CompassAcquirer.Stop();
                            break;
                        case DataType.Gyroscope:
                            GyroscopeAcquirer.Stop();
                            break;
                        case DataType.Motion:
                            MotionAcquirer.Stop();
                            break;
                        case DataType.Touch:
                            TouchAcquirer.Stop(dataType);
                            break;
                        case DataType.Tap:
                        case DataType.DoubleTap:
                        case DataType.Hold:
                        case DataType.Flick:
                        case DataType.FreeDrag:
                        case DataType.HorizontalDrag:
                        case DataType.VerticalDrag:
                        case DataType.DragComplete:
                        case DataType.Pinch:
                        case DataType.PinchComplete:
                            GestureAcquirer.Stop(dataType);
                            break;
                        case DataType.CustomDrag:
                            TouchAcquirer.Stop(dataType);
                            break;
                        case DataType.CustomDragComplete:
                            // no need to explicitly stop this, as this data type needs to be sent manually
                            // using the "AddData" method.
                            break;
                        case DataType.Text:
                            // we do not have an explicit text data source
                            // since this requires some sort of UI/user interaction with a text box etc.
                            // text is added from external sources using the "AddData" method.
                            // we do however remove this to the active data types to signal to the outside 
                            // that actions should be taken to disable user text input
                            break;
                    }
                }

                // raise event
                RaiseDataTypesChangedEvent();
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException(string.Format("Error while stopping acquisition of data {0}: {1}", dataType, ex.Message), ex);
            }
        }

        /// <summary>
        /// Pauses acquisition of a certain data type.
        /// </summary>
        /// <param name="dataType">The type of the data to pause the acquisition for.</param>
        public void PauseAcquisition(DataType dataType)
        {
            // pausing at the moment works in the following way:
            // we keep track of paused data types and to not report them using 
            // DataAcquired event; however, the acquirer for the data type
            // still is running and processes that type of data. It's just not used.
            // A performance optimization for the future could be to actually pause
            // the data acquisition too.

            // synchronize so that start/stop/pause/resume do not overlap
            lock (_locker)
            {
                if (!_activeDataTypes.Contains(dataType))
                {
                    return;
                }

                _logger.Trace("Pause acquisition of {0}", dataType);

                // add it to the paused list
                if (!_pausedDataTypes.Contains(dataType))
                {
                    _pausedDataTypes.Add(dataType);
                }
            }
        }

        /// <summary>
        /// Resumes acquisition of a certain data type.
        /// </summary>
        /// <param name="dataType">The type of data to resume the acquisition for.</param>
        public void ResumeAcquisition(DataType dataType)
        {
            // synchronize so that start/stop/pause/resume do not overlap
            lock (_locker)
            {
                if (!_pausedDataTypes.Contains(dataType))
                {
                    return;
                }

                _logger.Trace("Resuming acquisition of {0}", dataType);

                _pausedDataTypes.Remove(dataType);
            }
        }

        /// <summary>
        /// Shuts down the data source and cleans up all resources.
        /// </summary>
        public void Shutdown()
        {
            _logger.Trace("Shutting down");

            // get a copy of the active types (to avoid issues with the _activeDataTypes being manipulated during iteration)
            var activeDataTypes = GetActiveDataTypes();
            foreach (var dataType in activeDataTypes)
            {
                StopAcquisition(dataType);
            }
        }

        /// <summary>
        /// Gets the currently active data types, including the paused data types.
        /// </summary>
        /// <returns>
        /// A collection of the data types that are currently set to be acquired, including the paused data types.
        /// </returns>
        public IEnumerable<DataType> GetActiveDataTypes()
        {
            _logger.Trace("Someone is retrieving a copy of the active data types");

            //lock (_locker)
            //{
                // return a copy of our internal data
                return _activeDataTypes.ToArray();
            //}
        }

        /// <summary>
        /// Gets the paused data types only.
        /// </summary>
        /// <returns>
        /// A collection containing the currently paused data types.
        /// </returns>
        public IEnumerable<DataType> GetPausedDataTypes()
        {
            _logger.Trace("Someone is retrieving a copy of the paused data types");

            // we lock on the active data types here, because that is what is used all over the place
            lock (_locker)
            {
                // return a copy of our internal data
                return _pausedDataTypes.ToArray();
            }
        }

        /// <summary>
        /// Gets capabilities information about the device used for data acquisition.
        /// </summary>
        /// <returns>
        /// A <c>ControllerInfoData</c> object that contains the capabilities information of the device.
        /// </returns>
        public ControllerInfoData GetControllerInfoData()
        {
            _logger.Trace("Creating and returning information about the controller capabilities");

            // create info
            var data = new ControllerInfoData();
            data.ClientVersion = Constants.ControllerVersion;

            // add device caps
            data.IsTouchSupported = true; // touch is supported on all devices
            data.IsAccelerometerSupported = Accelerometer.IsSupported;
            data.IsCompassSupported = Compass.IsSupported;
            data.IsGyroscopeSupported = Gyroscope.IsSupported;
            data.IsMotionSupported = Motion.IsSupported;

            // we want the logical dimensions that are valid for landscape mode
            data.DisplayWidth = DeviceInfo.Current.LogicalScreenWidth;
            data.DisplayHeight = DeviceInfo.Current.LogicalScreenHeight;

            return data;
        }

        /// <summary>
        /// Configures the data acquisition with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        public void Configure(ControllerConfiguration configuration)
        {
            _logger.Trace("Configuring from configuration");

            TouchAcquirer.InputMargin = configuration.TouchInputMargin;
            GestureAcquirer.InputMargin = configuration.TouchInputMargin;
            AccelerometerAcquirer.MinDataRate = configuration.MinAccelerometerDataRate;
            CompassAcquirer.MinDataRate = configuration.MinCompassDataRate;
            GyroscopeAcquirer.MinDataRate = configuration.MinGyroscopeDataRate;
            MotionAcquirer.MinDataRate = configuration.MinMotionDataRate;
        }

        /// <summary>
        /// Manually adds new data to the data source to be reported to consumers.
        /// This is mandatory for certain data types like <c>Text</c> which cannot be acquired automatically.
        /// </summary>
        /// <param name="dataMessage">The data message to be reported to consumers of the data source.</param>
        public void AddData(DataMessage dataMessage)
        {
            try
            {
                _logger.Trace("Manually adding data of type {0} using AddData", dataMessage.DataType);

                // no synchronization needed here, because it is done
                // when the DataAcquired event is raised

                if (dataMessage.DataType == DataType.Text)
                {
                    // we need to specially prepare text due to the message size constraints
                    var textMessage = dataMessage as TextData;
                    if (textMessage != null)
                    {
                        AddData(textMessage.Text);
                    }
                }
                else
                {
                    // for all other types of data, we simply pass through what we get
                    RaiseDataAcquiredEvent(dataMessage);
                }
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException(string.Format("Error while adding data {0}: {1}", dataMessage.DataType, ex.Message), ex);
            }
        }

        private void AddData(string text)
        {
            // we split the text into multiple data messages if necessary
            // since we're dealing with unicode (UTF8) data here, the _absolute_
            // worst case is that we have a (4 * character count) byte size.
            // so what we do is use the maximum payload for a data message divided by 4
            // to be on the safe side.
            var chunkSize = (int)Math.Floor(DataMessage.MaxPayloadLength / 4.0); // probably 62 at the moment
            var chunks = (int)Math.Ceiling(text.Length / (double)chunkSize);

            // report one message for each chunk
            for (int i = 0; i < chunks; i++)
            {
                var skip = i * chunkSize;
                var remainingSize = text.Length - skip;

                // create fragment
                string textFragment;
                if (remainingSize > chunkSize)
                {
                    textFragment = text.Substring(skip, chunkSize);
                }
                else
                {
                    textFragment = text.Substring(skip);
                }

                // create message and report
                var data = new TextData();
                data.Text = textFragment;
                RaiseDataAcquiredEvent(data);
            }
        }

        #endregion
    }
}
