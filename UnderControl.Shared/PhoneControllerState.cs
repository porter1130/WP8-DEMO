
namespace UnderControl.Shared
{
    /// <summary>
    /// The possible states of the phone controller, both for the client and server side.
    /// </summary>
    public enum PhoneControllerState
    {
        /// <summary>
        /// The controller has been created and basic configuration has been performed,
        /// but no networking operations have been started yet.
        /// </summary>
        Created,

        /// <summary>
        /// The controller has started networking operations and is in a pending state,
        /// for example listening for incoming client connection requests, or in the progress
        /// of connecting to a remote server.
        /// </summary>
        Initialized,

        /// <summary>
        /// The controller is ready to be used, which means it has successfully connected
        /// to a remote server, or successfully accepted an incoming client request. The
        /// data and control channels are fully functional and can be used to transfer
        /// commands and data.
        /// </summary>
        Ready,

        /// <summary>
        /// An unrecoverable error occurred and transitioned the controller into this state.
        /// No more sending or receiving of data is possible. Applications should shut down
        /// the controller to ensure cleanup of all resources. Shutting down a controller from
        /// the error state ultimately will result in a transition to the <c>Closed</c> state,
        /// which allows re-initialization.
        /// Please note that applications should consider capturing the <c>Error</c>
        /// event of a controller to get more information about the actual error that occurred,
        /// instead of solely relying on detecting state transitions to the <c>Error</c> state.
        /// </summary>
        Error,

        /// <summary>
        /// The controller has started shutting down.
        /// </summary>
        Closing,

        /// <summary>
        /// The controller has finished shutting down. All resources have been successfully 
        /// cleaned up. A controller in the <c>Closed</c> state can be re-initialized and 
        /// is logically equivalent to a controller in the <c>Created</c> state.
        /// </summary>
        Closed
    }
}
