namespace UnderControl.Shared.ControlCommands
{
    /// <summary>
    /// The type of action the control command describes.
    /// </summary>
    public enum ControlCommandAction
    {
        /// <summary>
        /// No command action, used only for configuration commands.
        /// </summary>
        None,

        /// <summary>
        /// Start command action, triggers acquisition of a certain data type.
        /// </summary>
        Start,

        /// <summary>
        /// Stop command action, stops acquisition of a certain data type.
        /// </summary>
        Stop
    }
}