
using System.IO;
using UnderControl.Shared.Data;

namespace UnderControl.Shared
{
    /// <summary>
    /// A data container for all possible configuration options for the phone device and client side library.
    /// </summary>
    public class ControllerConfiguration
    {
        /// <summary>
        /// Determines whether the phone client tries to reconnect to the last known server 
        /// automatically when the application is activated. A reconnect attempt is not made
        /// when a new instance of the client is launched or when the client wasn't connected on deactivation.
        /// </summary>
        public bool AutoReconnectOnActivation
        {
            get;
            set;
        }

        /// <summary>
        /// The minimum number of time that has to elapse between messages to be sent from the phone.
        /// Setting this to zero means that the phone tries to send the messages as fast as possible
        /// and immediately when they are created.
        /// </summary>
        public int MinMillisecondsBetweenMessages
        {
            get;
            set;
        }

        /// <summary>
        /// The rate (in milliseconds) new data is aquired from the accelerometer, if applicable.
        /// The actual lower bound of this value depends on the minimum rate the hardware sensor is capable of.
        /// </summary>
        public int MinAccelerometerDataRate
        {
            get;
            set;
        }

        /// <summary>
        /// The rate (in milliseconds) new data is aquired from the compass, if applicable.
        /// The actual lower bound of this value depends on the minimum rate the hardware sensor is capable of.
        /// </summary>
        public int MinCompassDataRate
        {
            get;
            set;
        }

        /// <summary>
        /// The rate (in milliseconds) new data is aquired from the gyroscope, if applicable.
        /// The actual lower bound of this value depends on the minimum rate the hardware sensor is capable of.
        /// </summary>
        public int MinGyroscopeDataRate
        {
            get;
            set;
        }

        /// <summary>
        /// The rate (in milliseconds) new data is aquired from the combined motion API, if applicable.
        /// The actual lower bound of this value depends on the minimum rate the hardware sensors are capable of.
        /// </summary>
        public int MinMotionDataRate
        {
            get;
            set;
        }

        /// <summary>
        /// A margin, in pixels, subtracted from the active touch area of the phone screen to make it 
        /// simpler for the user to reach the touch extremes (edges) of the screen in absolute position mode,
        /// and to avoid overlapping with the additional controls available for special input.
        /// </summary>
        public Thickness TouchInputMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether tracing (using NLog's LogReceiverTarget) is enabled.
        /// </summary>
        public bool EnableTracing
        {
            get;
            set;
        }

        /// <summary>
        /// The endpoint address that should be used for NLog's LogReceiverTarget when tracing is enabled.
        /// </summary>
        public string TracingEndpointAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerConfiguration"/> class using default values for the configuration.
        /// </summary>
        public ControllerConfiguration()
        {
            // create default configuration values
            AutoReconnectOnActivation = false;
            TouchInputMargin = new Thickness(90, 20, 20, 20); // this is the default for the standard WP7 client so it can show the special buttons
            MinAccelerometerDataRate = 0;
            MinCompassDataRate = 0;
            MinGyroscopeDataRate = 0;
            MinMotionDataRate = 0;
            MinMillisecondsBetweenMessages = 0; // as fast as possible
            EnableTracing = false;
            TracingEndpointAddress = null;
        }

        /// <summary>
        /// Serializes the configuration to the given binary writer.
        /// </summary>
        /// <param name="bw">The binary writer to use.</param>
        internal void Serialize(BinaryWriter bw)
        {
            bw.Write(AutoReconnectOnActivation);
            bw.Write(MinMillisecondsBetweenMessages);
            bw.Write(MinAccelerometerDataRate);
            bw.Write(MinCompassDataRate);
            bw.Write(MinGyroscopeDataRate);
            bw.Write(MinMotionDataRate);
            bw.Write(TouchInputMargin.Left);
            bw.Write(TouchInputMargin.Top);
            bw.Write(TouchInputMargin.Right);
            bw.Write(TouchInputMargin.Bottom);
            bw.Write(EnableTracing);
            bw.Write(TracingEndpointAddress ?? string.Empty);
        }

        /// <summary>
        /// Deserializes the configuration from a given binary reader.
        /// </summary>
        /// <param name="br">The binary reader to use.</param>
        internal void Deserialize(BinaryReader br)
        {
            AutoReconnectOnActivation = br.ReadBoolean();
            MinMillisecondsBetweenMessages = br.ReadInt32();
            MinAccelerometerDataRate = br.ReadInt32();
            MinCompassDataRate = br.ReadInt32();
            MinGyroscopeDataRate = br.ReadInt32();
            MinMotionDataRate = br.ReadInt32();
            TouchInputMargin = new Thickness();
            TouchInputMargin.Left = br.ReadDouble();
            TouchInputMargin.Top = br.ReadDouble();
            TouchInputMargin.Right = br.ReadDouble();
            TouchInputMargin.Bottom = br.ReadDouble();
            EnableTracing = br.ReadBoolean();
            TracingEndpointAddress = br.ReadString();
        }
    }
}
