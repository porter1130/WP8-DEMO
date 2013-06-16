using System.IO;

namespace UnderControl.Shared.ControlCommands
{
    /// <summary>
    /// A special control command used to transport configuration information from the server to the client.
    /// </summary>
    public class ConfigurationControlCommand : ControlCommand
    {
        private ControllerConfiguration _configuration;

        /// <summary>
        /// Gets or sets the configuration that is send from the server to the client..
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public ControllerConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new ControllerConfiguration();
                }

                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationControlCommand"/> class.
        /// </summary>
        public ConfigurationControlCommand()
            : base(DataType.Configuration, ControlCommandAction.None)
        {
            Configuration = new ControllerConfiguration();
        }

        /// <summary>
        /// Adds custom configuration data to the raw representation of the command.
        /// </summary>
        /// <param name="bw">The binary writer used to write the raw represenation of the command.</param>
        protected override void WriteData(BinaryWriter bw)
        {
            Configuration.Serialize(bw);
        }

        /// <summary>
        /// Reads back the custom configuration data from the raw command representation.
        /// </summary>
        /// <param name="br">The binary reader used to read the raw command representation.</param>
        internal override void ReadData(BinaryReader br)
        {
            Configuration.Deserialize(br);
        }
    }
}