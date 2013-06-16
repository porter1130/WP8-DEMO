
namespace UnderControl.Shared
{
    /// <summary>
    /// A collection of global constants used throughout the library.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The version of the library. This is increased every time a change in the protocol is introduced
        /// and enables servers to detect connections from incompatible and outdated clients. Those connection
        /// attempts should be rejected.
        /// </summary>
        public const int ControllerVersion = 17;

        /// <summary>
        /// The timeout used for multicast broadcasting.
        /// </summary>
        public const int MulticastTimeout = 5000;

        /// <summary>
        /// The multicast group address used for server discovery.
        /// </summary>
        public const string MulticastGroupAddress = "224.0.0.42";

        /// <summary>
        /// The multicast group port used for server discovery.
        /// </summary>
        public const int MulticastGroupPort = 52275;

        /// <summary>
        /// A token used for multicast discovery to avoid conflicts with other software using the same multicast group settings.
        /// </summary>
        public const string MulticastDiscoveryToken = "{FDFA6550-56A6-4DBD-A7F2-A8764A7AD392}";

        /// <summary>
        /// The maximum size of a multicast message.
        /// </summary>
        public const int MulticastMaxMessageSize = 64;

        /// <summary>
        /// The control channel port.
        /// </summary>
        public const int CommunicationControlChannelPort = 42002;

        /// <summary>
        /// The data channel port.
        /// </summary>
        public const int CommunicationDataChannelPort = 42003;
    }
}
