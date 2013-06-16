namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A thickness data container class,
    /// necessary to overcome the limitations of portable class libraries.
    /// </summary>
    public class Thickness
    {
        /// <summary>
        /// Gets or sets the thickness amount at the left side.
        /// </summary>
        public double Left
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the thickness amount at the top side.
        /// </summary>
        public double Top
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the thickness amount at the right side.
        /// </summary>
        public double Right
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the thickness amount at the bottom side.
        /// </summary>
        public double Bottom
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Thickness"/> class.
        /// </summary>
        public Thickness()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Thickness"/> class for a given set of thickness amounts.
        /// </summary>
        /// <param name="left">The amount of thickness at the left side.</param>
        /// <param name="top">The amount of thickness at the top side.</param>
        /// <param name="right">The amount of thickness at the right side.</param>
        /// <param name="bottom">The amount of thickness at the bottom side.</param>
        public Thickness(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}