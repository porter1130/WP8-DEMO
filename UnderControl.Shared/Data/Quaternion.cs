namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A quaternion data container class,
    /// necessary to overcome the limitations of portable class libraries.
    /// </summary>
    public class Quaternion
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
        /// Gets or sets the W coordinate.
        /// </summary>
        public float W
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quaternion"/> class.
        /// </summary>
        public Quaternion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quaternion"/> class with a given set of coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <param name="w">The W coordinate.</param>
        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}