using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Represents a data message that contains the capabilities of the device used for data acquisition, among other information.
    /// </summary>
    public sealed class ControllerInfoData : DataMessage
    {
        /// <summary>
        /// Gets or sets the version of the UnderControl library used at the client side.
        /// A server should check whether the client version is the same as the expected version
        /// an reject connection attempts of incompatible/outdated clients.
        /// </summary>
        /// <value>
        /// The client version.
        /// </value>
        public int ClientVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data source supports touch input.
        /// This is true for all Windows Phone 7 devices.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if touch input is supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsTouchSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data source supports accelerometer input.
        /// This is true for all Windows Phone 7 devices.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if accelerometer iput is supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsAccelerometerSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data source supports compass input.
        /// This is true for all Windows Phone 7 devices.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if compass input is supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompassSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data source supports gyroscope input.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if gyroscope input is supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsGyroscopeSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data source supports using the combined motion API.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the combined motion API is supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsMotionSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the logical display width. Since the library only supports landscape mode,
        /// this is the physical display height. Typically, this value is 800.0.
        /// </summary>
        /// <value>
        /// The logical display width.
        /// </value>
        public double DisplayWidth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the logical display height. Since the library only supports landscape mode,
        /// this is the physical display width. Typically, this value is 480.0.
        /// </summary>
        /// <value>
        /// The logical display height.
        /// </value>
        public double DisplayHeight
        {
            get;
            set;
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>ControllerInfo</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.ControllerInfo;
            }
        }

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public override bool MustBeDelivered
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Adds custom data to the raw representation of the data message.
        /// </summary>
        /// <param name="writer">The binary writer used to create the raw representation of the data message.</param>
        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(ClientVersion);
            writer.Write(IsTouchSupported);
            writer.Write(IsAccelerometerSupported);
            writer.Write(IsGyroscopeSupported);
            writer.Write(IsCompassSupported);
            writer.Write(IsMotionSupported);
            writer.Write(DisplayWidth);
            writer.Write(DisplayHeight);
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
            ClientVersion = reader.ReadInt32();
            IsTouchSupported = reader.ReadBoolean();
            IsAccelerometerSupported = reader.ReadBoolean();
            IsGyroscopeSupported = reader.ReadBoolean();
            IsCompassSupported = reader.ReadBoolean();
            IsMotionSupported = reader.ReadBoolean();
            DisplayWidth = reader.ReadDouble();
            DisplayHeight = reader.ReadDouble();
        }

        #endregion
    }
}
