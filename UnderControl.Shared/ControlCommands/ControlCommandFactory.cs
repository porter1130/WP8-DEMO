using System.IO;

namespace UnderControl.Shared.ControlCommands
{
    /// <summary>
    /// Used to create control command implementations from given <c>DataType</c> and <c>ControlCommandAction</c> combinations,
    /// or from raw control command representations.
    /// </summary>
    public static class ControlCommandFactory
    {
        /// <summary>
        /// Creates a control command from raw data.
        /// </summary>
        /// <param name="rawCommand">The raw control command data.</param>
        /// <returns>A control command that contains the information from the raw representation</returns>
        public static IControlCommand CreateFromRawCommand(byte[] rawCommand)
        {
            return CreateFromRawCommand(rawCommand, 0);
        }

        /// <summary>
        /// Creates a control command from raw data, starting at a certain offset in the raw data.
        /// </summary>
        /// <param name="rawCommand">The raw control command data.</param>
        /// <param name="startIndex">The start index to extract a control command at.</param>
        /// <returns>A control command that contains the information from the raw representation</returns>
        public static IControlCommand CreateFromRawCommand(byte[] rawCommand, int startIndex)
        {
            // inspect command length
            var length = rawCommand[startIndex];

            // build the memory stream from the range
            var ms = new MemoryStream(rawCommand, startIndex, length);
            var br = new BinaryReader(ms);

            // consume the already known length byte
            length = br.ReadByte();

            // extract the remaining meta data
            var dataType = (DataType)br.ReadInt32();
            var action = (ControlCommandAction)br.ReadInt32();

            ControlCommand command = null;
            if (dataType == DataType.Configuration)
            {
                command = new ConfigurationControlCommand();
            }
            else
            {
                command = new ControlCommand(dataType, action);
            }

            command.Length = length;
            command.ReadData(br);

            br.Dispose();

            return command;
        }

        /// <summary>
        /// Creates a control command from the give data type and action.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="action">The action. Ignored if a data type of <c>Configuration</c> is used.</param>
        /// <returns>A control command for the given data type and action.</returns>
        public static IControlCommand CreateCommand(DataType dataType, ControlCommandAction action)
        {
            if (dataType == DataType.Configuration)
            {
                return new ConfigurationControlCommand();
            }
            else
            {
                return new ControlCommand(dataType, action);
            }
        }

        /// <summary>
        /// A convenient method to create a configuration control command.
        /// </summary>
        /// <returns>A control command of type <c>ConfigurationControlCommand</c>.</returns>
        public static ConfigurationControlCommand CreateConfigurationCommand()
        {
            return CreateCommand(DataType.Configuration, ControlCommandAction.None) as ConfigurationControlCommand;
        }
    }
}
