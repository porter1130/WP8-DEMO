
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;
using UnderControl.Shared;
using UnderControl.Shared.Data;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// An acquirer that controls the acquisition of gesture data.
    /// </summary>
    internal sealed class GestureAcquirer : PeriodicAcquirerBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Action<DataMessage> _dataAcquiredCallback;
        private readonly List<GestureType> _activeGestures = new List<GestureType>();
        private readonly Dictionary<DataType, GestureType> _dataTypeGestureTypeMapping = new Dictionary<DataType, GestureType>
                                                                                             {
                                                                                                 { DataType.Tap, GestureType.Tap },
                                                                                                 { DataType.DoubleTap, GestureType.DoubleTap },
                                                                                                 { DataType.Hold, GestureType.Hold },
                                                                                                 { DataType.Flick, GestureType.Flick },
                                                                                                 { DataType.FreeDrag, GestureType.FreeDrag },
                                                                                                 { DataType.HorizontalDrag, GestureType.HorizontalDrag },
                                                                                                 { DataType.VerticalDrag, GestureType.VerticalDrag },
                                                                                                 { DataType.DragComplete, GestureType.DragComplete },
                                                                                                 { DataType.Pinch, GestureType.Pinch },
                                                                                                 { DataType.PinchComplete, GestureType.PinchComplete }
                                                                                             };

        /// <summary>
        /// Gets or sets the input margin that defines the active touch area used for data acquisition.
        /// </summary>
        public Thickness InputMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureAcquirer"/> class.
        /// </summary>
        /// <param name="dataAcquiredCallback">The data acquired callback that is invoked when new data has been acquired.</param>
        public GestureAcquirer(Action<DataMessage> dataAcquiredCallback)
        {
            _dataAcquiredCallback = dataAcquiredCallback;
        }

        /// <summary>
        /// Starts acquisition of the specified data type, if supported.
        /// </summary>
        /// <param name="dataType">The type of data to start acquisition for.</param>
        public void Start(DataType dataType)
        {
            if (!_dataTypeGestureTypeMapping.ContainsKey(dataType))
            {
                return;
            }

            // get gesture type
            var gestureType = _dataTypeGestureTypeMapping[dataType];

            // check if we're already reporting this gesture
            if (_activeGestures.Contains(gestureType))
            {
                return;
            }

            _logger.Trace("Starting acquisition of gesture type {0}", dataType);

            // add gesture type
            _activeGestures.Add(gestureType);

            // set the gestures we're interested in
            GestureType gestures = GestureType.None;
            foreach (var gesture in _activeGestures)
            {
                gestures = gestures | gesture;
            }
            TouchPanel.EnabledGestures = gestures;

            // do we need to actually start something?
            if (_activeGestures.Count == 1)
            {
                Enlist(ProcessGestures);
            }
        }

        /// <summary>
        /// Stops acquisition of the specified data type.
        /// </summary>
        /// <param name="dataType">The type of data to stop acquisition for.</param>
        public void Stop(DataType dataType)
        {
            if (!_dataTypeGestureTypeMapping.ContainsKey(dataType))
            {
                return;
            }

            // get gesture type
            var gestureType = _dataTypeGestureTypeMapping[dataType];

            // check if we're actually reporting this gesture
            if (!_activeGestures.Contains(gestureType))
            {
                return;
            }

            _logger.Trace("Stopping acquisition of gesture type {0}", dataType);

            // remove gesture type
            _activeGestures.Remove(gestureType);

            // set the gestures we're interested in
            GestureType gestures = GestureType.None;
            foreach (var gesture in _activeGestures)
            {
                gestures = gestures | gesture;
            }
            TouchPanel.EnabledGestures = gestures;

            // if we are not interested in any gestures anymore, unsubscribe
            if (_activeGestures.Count == 0)
            {
                Remove(ProcessGestures);
            }
        }

        private void ProcessGestures()
        {
            while (TouchPanel.IsGestureAvailable)
            {
                var gesture = TouchPanel.ReadGesture();

                // only report desired gestures
                if (!_activeGestures.Contains(gesture.GestureType))
                {
                    continue;
                }

                // check to see what we need to do
                switch (gesture.GestureType)
                {
                    case GestureType.Tap:
                        ProcessTap(gesture);
                        break;
                    case GestureType.DoubleTap:
                        ProcessDoubleTap(gesture);
                        break;
                    case GestureType.Hold:
                        ProcessHold(gesture);
                        break;
                    case GestureType.Flick:
                        ProcessFlick(gesture);
                        break;
                    case GestureType.FreeDrag:
                        ProcessDrag(gesture, new FreeDragData());
                        break;
                    case GestureType.HorizontalDrag:
                        ProcessDrag(gesture, new HorizontalDragData());
                        break;
                    case GestureType.VerticalDrag:
                        ProcessDrag(gesture, new VerticalDragData());
                        break;
                    case GestureType.DragComplete:
                        ProcessDragComplete(gesture);
                        break;
                    case GestureType.Pinch:
                        ProcessPinch(gesture);
                        break;
                    case GestureType.PinchComplete:
                        ProcessPinchComplete(gesture);
                        break;
                }
            }
        }

        private void ProcessTap(GestureSample gesture)
        {
            // first convert to logical coordinates
            var location = new Vector2();
            CoordinateSystemHelper.CalculateLogicalPosition(location, gesture.Position);

            // make sure the position is valid
            if (!CoordinateSystemHelper.IsInActiveArea(location, InputMargin))
            {
                return;
            }

            // project
            CoordinateSystemHelper.ProjectToActiveArea(location, InputMargin);

            _logger.Trace("Processing tap gesture");

            var data = new TapData();

            // set values
            data.TouchPoint = new TouchPoint
            {
                // Id and State are not really used here
                Id = 0,
                State = TouchPointState.Released,
                Location = location
            };
            data.Timestamp = new DateTimeOffset(gesture.Timestamp.Ticks, TimeSpan.Zero);

            // notify
            _dataAcquiredCallback(data);
        }

        private void ProcessDoubleTap(GestureSample gesture)
        {
            // first convert to logical coordinates
            var location = new Vector2();
            CoordinateSystemHelper.CalculateLogicalPosition(location, gesture.Position);

            // make sure the position is valid
            if (!CoordinateSystemHelper.IsInActiveArea(location, InputMargin))
            {
                return;
            }

            // project
            CoordinateSystemHelper.ProjectToActiveArea(location, InputMargin);

            _logger.Trace("Processing double tap gesture");

            var data = new DoubleTapData();

            // set values
            data.TouchPoint = new TouchPoint
            {
                // Id and State are not really used here
                Id = 0,
                State = TouchPointState.Released,
                Location = location
            };
            data.Timestamp = new DateTimeOffset(gesture.Timestamp.Ticks, TimeSpan.Zero);

            // notify
            _dataAcquiredCallback(data);
        }

        private void ProcessHold(GestureSample gesture)
        {
            // first convert to logical coordinates
            var location = new Vector2();
            CoordinateSystemHelper.CalculateLogicalPosition(location, gesture.Position);

            // make sure the position is valid
            if (!CoordinateSystemHelper.IsInActiveArea(location, InputMargin))
            {
                return;
            }

            // project
            CoordinateSystemHelper.ProjectToActiveArea(location, InputMargin);

            _logger.Trace("Processing hold gesture");

            var data = new HoldData();

            // set values
            data.TouchPoint = new TouchPoint
                                  {
                                      // Id and State are not really used here
                                      Id = 0,
                                      State = TouchPointState.Released,
                                      Location = location
                                  };
            data.Timestamp = new DateTimeOffset(gesture.Timestamp.Ticks, TimeSpan.Zero);

            // notify
            _dataAcquiredCallback(data);
        }

        private void ProcessFlick(GestureSample gesture)
        {
            _logger.Trace("Processing flick gesture");

            var data = new FlickData();

            // set values
            data.Timestamp = new DateTimeOffset(gesture.Timestamp.Ticks, TimeSpan.Zero);
            data.Delta = new Vector2();
            CoordinateSystemHelper.AdjustLogicalAxis(data.Delta, gesture.Delta);

            // notify
            _dataAcquiredCallback(data);
        }

        private void ProcessDrag(GestureSample gesture, DragDataBase data)
        {
            // first convert to logical coordinates
            var location = new Vector2();
            CoordinateSystemHelper.CalculateLogicalPosition(location, gesture.Position);

            // make sure the position is valid
            if (!CoordinateSystemHelper.IsInActiveArea(location, InputMargin))
            {
                return;
            }

            // project
            CoordinateSystemHelper.ProjectToActiveArea(location, InputMargin);

            _logger.Trace("Processing free drag gesture");

            // set values
            data.TouchPoint = new TouchPoint
                                  {
                                      // Id and State are not really used here
                                      Id = 0,
                                      State = TouchPointState.Moved,
                                      Location = location
                                  };
            data.Delta = new Vector2();
            CoordinateSystemHelper.AdjustLogicalAxis(data.Delta, gesture.Delta);
            data.Timestamp = new DateTimeOffset(gesture.Timestamp.Ticks, TimeSpan.Zero);

            // notify
            _dataAcquiredCallback(data);
        }

        private void ProcessDragComplete(GestureSample gesture)
        {
            _logger.Trace("Processing drag complete gesture");

            // this gesture does not provide any additional data
            var data = new DragCompleteData();

            // notify
            _dataAcquiredCallback(data);
        }

        private void ProcessPinch(GestureSample gesture)
        {
            // convert first and second positions to logical coordinates
            var location = new Vector2();
            CoordinateSystemHelper.CalculateLogicalPosition(location, gesture.Position);
            var location2 = new Vector2();
            CoordinateSystemHelper.CalculateLogicalPosition(location2, gesture.Position2);

            // make sure the positions are valid
            if (!CoordinateSystemHelper.IsInActiveArea(location, InputMargin) || !CoordinateSystemHelper.IsInActiveArea(location2, InputMargin))
            {
                return;
            }

            // project
            CoordinateSystemHelper.ProjectToActiveArea(location, InputMargin);
            CoordinateSystemHelper.ProjectToActiveArea(location2, InputMargin);

            _logger.Trace("Processing pinch gesture");

            // set values
            var data = new PinchData();
            data.TouchPoint = new TouchPoint
                                  {
                                      // Id and State are not really used here
                                      Id = 0,
                                      State = TouchPointState.Moved,
                                      Location = location
                                  };
            data.TouchPoint2 = new TouchPoint
                                   {
                                       // Id and State are not really used here too
                                       Id = 0,
                                       State = TouchPointState.Moved,
                                       Location = location2
                                   };

            data.Delta = new Vector2();
            CoordinateSystemHelper.AdjustLogicalAxis(data.Delta, gesture.Delta);
            data.Delta2 = new Vector2();
            CoordinateSystemHelper.AdjustLogicalAxis(data.Delta2, gesture.Delta2);

            data.Timestamp = new DateTimeOffset(gesture.Timestamp.Ticks, TimeSpan.Zero);

            // notify
            _dataAcquiredCallback(data);
        }

        private void ProcessPinchComplete(GestureSample gesture)
        {
            _logger.Trace("Processing pinch complete gesture");

            // this gesture does not provide any additional data
            var data = new PinchCompleteData();

            // notify
            _dataAcquiredCallback(data);
        }
    }
}
