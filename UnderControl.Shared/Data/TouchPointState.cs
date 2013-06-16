namespace UnderControl.Shared.Data
{
    /// <summary>
    /// The possible states of a touch point.
    /// </summary>
    public enum TouchPointState
    {
        /// <summary>
        /// The touch point is invalid and should be ignored.
        /// </summary>
        Invalid,

        /// <summary>
        /// The touch point is a follow-up touch point that belongs to a series of related touch events.
        /// </summary>
        Moved,

        /// <summary>
        /// The touch point is the initial touch event for a potential series of upcoming touch events.
        /// </summary>
        Pressed,

        /// <summary>
        /// The touch point is the last touch point in a series of touch events.
        /// </summary>
        Released
    }
}