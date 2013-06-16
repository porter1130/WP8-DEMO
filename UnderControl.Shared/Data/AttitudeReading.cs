using System;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A attitude reading data container class,
    /// necessary to overcome the limitations of portable class libraries.
    /// </summary>
    public class AttitudeReading
    {
        /// <summary>
        /// Gets or sets the pitch value.
        /// </summary>
        public float Pitch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the quaternion.
        /// </summary>
        public Quaternion Quaternion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the roll value.
        /// </summary>
        public float Roll
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rotation matrix.
        /// </summary>
        public Matrix RotationMatrix
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time stamp of the reading.
        /// </summary>
        public DateTimeOffset TimeStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the yaw value.
        /// </summary>
        public float Yaw
        {
            get;
            set;
        }
    }
}
