namespace UnderControl.Shared
{
    /// <summary>
    /// The possible data types for data acquisition on the phone.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Basic controller information (capabilities).
        /// </summary>
        ControllerInfo,

        /// <summary>
        /// Configuration information for the client-side library.
        /// </summary>
        Configuration,

        /// <summary>
        /// Accelerometer data.
        /// </summary>
        Accelerometer,

        /// <summary>
        /// Compass data.
        /// </summary>
        Compass,

        /// <summary>
        /// Gyroscope data.
        /// </summary>
        Gyroscope,

        /// <summary>
        /// Combined motion API data.
        /// </summary>
        Motion,

        /// <summary>
        /// Raw (multi-)touch data.
        /// </summary>
        Touch,

        /// <summary>
        /// A tap gesture.
        /// </summary>
        Tap,

        /// <summary>
        /// A double-tap gesture.
        /// </summary>
        DoubleTap,

        /// <summary>
        /// A hold gesture.
        /// </summary>
        Hold,

        /// <summary>
        /// A flick gesture.
        /// </summary>
        Flick,

        /// <summary>
        /// An ongoing free-drag gesture.
        /// </summary>
        FreeDrag,

        /// <summary>
        /// An ongoing horizontal drag gesture.
        /// </summary>
        HorizontalDrag,

        /// <summary>
        /// An ongoing vertical drag gesture.
        /// </summary>
        VerticalDrag,

        /// <summary>
        /// Completion of a <c>FreeDrag</c>, <c>HorizontalDrag</c> or <c>VerticalDrag</c> gesture.
        /// </summary>
        DragComplete,

        /// <summary>
        /// An ongoing custom drag gesture.
        /// </summary>
        CustomDrag,

        /// <summary>
        /// Completion of a <c>CustomDrag</c> gesture.
        /// </summary>
        CustomDragComplete,

        /// <summary>
        /// An ongoing pinch gesture.
        /// </summary>
        Pinch,

        /// <summary>
        /// Completion of a <c>Pinch</c> gesture.
        /// </summary>
        PinchComplete,

        /// <summary>
        /// Text data.
        /// </summary>
        Text
    }
}