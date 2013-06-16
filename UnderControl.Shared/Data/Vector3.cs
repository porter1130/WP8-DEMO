namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A simple three-dimensional vector data container class,
    /// necessary to overcome the limitations of portable class libraries.
    /// </summary>
    public class Vector3
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
        /// Gets or sets the Z coordinate.
        /// </summary>
        public float Z
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3"/> class.
        /// </summary>
        public Vector3()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3"/> class.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}