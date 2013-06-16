namespace UnderControl.Shared.ControlCommands
{
    /// <summary>
    /// An interface that describes a control command which can be sent from the server to the client.
    /// </summary>
    public interface IControlCommand
    {
        /// <summary>
        /// Gets or sets the total length of the command, in bytes.
        /// </summary>
        /// <value>
        /// The length of the command in bytes.
        /// </value>
        byte Length
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        DataType DataType
        {
            get;
        }

        /// <summary>
        /// Gets the action that should be performed for the given data type.
        /// </summary>
        ControlCommandAction Action
        {
            get;
        }

        /// <summary>
        /// Creates a raw byte array representation of the command.
        /// </summary>
        /// <returns>A byte array containing a raw representation of the command.</returns>
        byte[] ToByteArray();
    }
}