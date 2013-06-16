namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A simple two-dimensional vector data container class,
    /// necessary to overcome the limitations of portable class libraries.
    /// </summary>
    public class Vector2
    {
        /// <summary>
        /// Gets or sets the X coordinate.
        /// </summary>
        public float X
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Y coordinate.
        /// </summary>
        public float Y
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2"/> class.
        /// </summary>
        public Vector2()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2"/> class.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}