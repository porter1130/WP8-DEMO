
namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Describes a raw touch point.
    /// </summary>
    public class TouchPoint
    {
        /// <summary>
        /// Gets or sets an id that can be used to identify this and related touch points.
        /// If this information is not used, the <c>Id</c> has a value of zero.
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location the touch event was reported at.
        /// </summary>
        public Vector2 Location
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the state of the touch point.
        /// </summary>
        public TouchPointState State
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPoint"/> class.
        /// </summary>
        public TouchPoint()
        {
            Location = new Vector2();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPoint"/> class for a given set of coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public TouchPoint(float x, float y)
        {
            Location = new Vector2(x, y);
        }
    }
}
