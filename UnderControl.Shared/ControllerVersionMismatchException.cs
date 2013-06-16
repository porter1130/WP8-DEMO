
namespace UnderControl.Shared
{
    /// <summary>
    /// An exception that signals that a connection request from a client was rejected due to a version mismatch.
    /// </summary>
    public class ControllerVersionMismatchException : PhoneControllerException
    {
        /// <summary>
        /// Gets the version the server expects from connecting clients.
        /// </summary>
        public int ServerVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the version that was reported by the connected client.
        /// </summary>
        public int ClientVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerVersionMismatchException"/> class.
        /// </summary>
        public ControllerVersionMismatchException(int serverVersion, int clientVersion)
            : base(
                string.Format("The controller version of the client does not match the expected version. Expected: {0}, but client is: {1}",
                        serverVersion,
                        clientVersion))
        {
            ServerVersion = serverVersion;
            ClientVersion = clientVersion;
        }
    }
}
