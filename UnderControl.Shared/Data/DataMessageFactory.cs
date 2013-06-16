using System;
using System.Diagnostics;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Creates data messages from raw representations.
    /// </summary>
    public static class DataMessageFactory
    {
        /// <summary>
        /// Creates a data message from raw message data.
        /// </summary>
        /// <param name="rawMessage">The raw message data.</param>
        /// <returns>A data message of correct type, recreated from the raw data.</returns>
        public static DataMessage CreateFromRawMessage(byte[] rawMessage)
        {
            return CreateFromRawMessage(rawMessage, 0);
        }

        /// <summary>
        /// Creates a data message from raw message data, starting at a given offset in the raw data.
        /// </summary>
        /// <param name="rawMessage">The raw message data.</param>
        /// <param name="start">The index the recreation of the data message should be started at.</param>
        /// <returns>A data message of correct type, recreated from the raw data.</returns>
        public static DataMessage CreateFromRawMessage(byte[] rawMessage, int start)
        {
            // result
            DataMessage result = null;

            // inspect type
            var dataType = (DataType)BitConverter.ToInt32(rawMessage, start + 1);

            // simple switch here
            switch (dataType)
            {
                case DataType.ControllerInfo:
                    result = new ControllerInfoData();
                    break;
                case DataType.Accelerometer:
                    result = new AccelerometerData();
                    break;
                case DataType.Compass:
                    result = new CompassData();
                    break;
                case DataType.Gyroscope:
                    result = new GyroscopeData();
                    break;
                case DataType.Motion:
                    result = new MotionData();
                    break;
                case DataType.Touch:
                    result = new TouchData();
                    break;
                case DataType.Tap:
                    result = new TapData();
                    break;
                case DataType.DoubleTap:
                    result = new DoubleTapData();
                    break;
                case DataType.Hold:
                    result = new HoldData();
                    break;
                case DataType.Flick:
                    result = new FlickData();
                    break;
                case DataType.FreeDrag:
                    result = new FreeDragData();
                    break;
                case DataType.HorizontalDrag:
                    result = new HorizontalDragData();
                    break;
                case DataType.VerticalDrag:
                    result = new VerticalDragData();
                    break;
                case DataType.DragComplete:
                    result = new DragCompleteData();
                    break;
                case DataType.CustomDrag:
                    result = new CustomDragData();
                    break;
                case DataType.CustomDragComplete:
                    result = new CustomDragCompleteData();
                    break;
                case DataType.Pinch:
                    result = new PinchData();
                    break;
                case DataType.PinchComplete:
                    result = new PinchCompleteData();
                    break;
                case DataType.Text:
                    result = new TextData();
                    break;
            }

            // fill from raw data
            if (result != null)
            {
                result.FromByteArray(rawMessage, start);
            }
            else
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            return result;
        }
    }
}
