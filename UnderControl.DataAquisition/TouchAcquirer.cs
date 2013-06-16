using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;
using UnderControl.Shared;
using UnderControl.Shared.Data;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// An acquirer that processes raw (multi-)touch input.
    /// </summary>
    internal sealed class TouchAcquirer : PeriodicAcquirerBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Action<DataMessage> _dataAcquiredCallback;

        private List<DataType> _activeDataTypes = new List<DataType>();
        private TouchPoint _lastDragTouchPoint = new TouchPoint();

        /// <summary>
        /// Gets or sets the input margin used to define the active area for touch input.
        /// </summary>
        public Thickness InputMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchAcquirer"/> class.
        /// </summary>
        /// <param name="dataAcquiredCallback">The data acquired callback invoked when new data has been acquired.</param>
        public TouchAcquirer(Action<DataMessage> dataAcquiredCallback)
        {
            _dataAcquiredCallback = dataAcquiredCallback;
        }

        /// <summary>
        /// Starts acquisition of the specified data type.
        /// Only data types <c>Touch</c> and <c>CustomDrag</c> are supported.
        /// </summary>
        /// <param name="dataType">The type of data to start acquisition for.</param>
        public void Start(DataType dataType)
        {
            _logger.Trace("Starting processing raw touch input for {0}", dataType);

            lock (_activeDataTypes)
            {
                if (!_activeDataTypes.Contains(dataType))
                {
                    _activeDataTypes.Add(dataType);
                    Enlist(ProcessTouch);
                }
            }
        }

        /// <summary>
        /// Stops acquisition of the specified data type.
        /// </summary>
        /// <param name="dataType">The type of data to stop acquisition for.</param>
        public void Stop(DataType dataType)
        {
            _logger.Trace("Stopping processing raw touch input");

            lock (_activeDataTypes)
            {
                if (_activeDataTypes.Contains(dataType))
                {
                    _activeDataTypes.Remove(dataType);
                }

                if (_activeDataTypes.Count == 0)
                {
                    // no need for further processing
                    Remove(ProcessTouch);
                }
            }
        }

        private void ProcessTouch()
        {
            var touchCollection = TouchPanel.GetState();
            if (touchCollection.Count > 0)
            {
                // convert list
                List<TouchPoint> touchPoints = ConvertTouchPoints(touchCollection);

                // can be zero if points are outside of bounds
                if (touchPoints.Count > 0)
                {
                    _logger.Trace("Processing raw touch/gesture input");

                    lock (_activeDataTypes)
                    {
                        // report touch?
                        if (_activeDataTypes.Contains(DataType.Touch))
                        {
                            var data = new TouchData(touchPoints);

                            // pass on data to the outside world
                            _dataAcquiredCallback(data);
                        }

                        // report drag?
                        if (_activeDataTypes.Contains(DataType.CustomDrag))
                        {
                            // use the first touch point
                            var touchPoint = touchPoints[0];

                            // create the data
                            var data = new CustomDragData();
                            data.TouchPoint = touchPoint;
                            data.Delta = new Vector2(touchPoint.Location.X - _lastDragTouchPoint.Location.X,
                                                     touchPoint.Location.Y - _lastDragTouchPoint.Location.Y);

                            // new reference
                            _lastDragTouchPoint = touchPoint;

                            // send
                            _dataAcquiredCallback(data);
                        }
                    }
                }
            }
        }

        private List<TouchPoint> ConvertTouchPoints(TouchCollection touchCollection)
        {
            List<TouchPoint> result = new List<TouchPoint>(touchCollection.Count);

            foreach (var touch in touchCollection)
            {
                // convert to logical coordinates
                var location = new Vector2();
                CoordinateSystemHelper.CalculateLogicalPosition(location, touch.Position);

                // check if this is valid
                if (!CoordinateSystemHelper.IsInActiveArea(location, InputMargin))
                {
                    continue;
                }

                // generate touch point
                var touchPoint = new TouchPoint();
                touchPoint.Id = touch.Id;
                touchPoint.State = ConvertState(touch.State);

                // project 
                CoordinateSystemHelper.ProjectToActiveArea(location, InputMargin);
                touchPoint.Location = location;

                result.Add(touchPoint);
            }

            return result;
        }

        private TouchPointState ConvertState(TouchLocationState state)
        {
            // a simple mapping between the built-in types and our own ones
            switch (state)
            {
                case TouchLocationState.Invalid:
                    return TouchPointState.Invalid;
                case TouchLocationState.Moved:
                    return TouchPointState.Moved;
                case TouchLocationState.Pressed:
                    return TouchPointState.Pressed;
                case TouchLocationState.Released:
                    return TouchPointState.Released;
                default:
                    return TouchPointState.Invalid;
            }
        }
    }
}
