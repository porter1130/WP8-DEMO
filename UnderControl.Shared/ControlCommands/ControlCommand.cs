using System;
using System.IO;
using System.Text;

namespace UnderControl.Shared.ControlCommands
{
    /// <summary>
    /// Describes a command that is sent from the server to control data acquisition on the client.
    /// </summary>
    public class ControlCommand : IControlCommand
    {
        #region Implementation of IControlCommand

        /// <summary>
        /// Gets or sets the total length of the command, in bytes.
        /// </summary>
        /// <value>
        /// The length of the command in bytes.
        /// </value>
        public byte Length
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
        public DataType DataType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the action that should be performed for the given data type.
        /// </summary>
        public ControlCommandAction Action
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a raw byte array representation of the command.
        /// </summary>
        /// <returns>
        /// A byte array containing a raw representation of the command.
        /// </returns>
        public byte[] ToByteArray()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms, Encoding.UTF8);

            // write the length place holder
            bw.Write((byte)0);

            // write the meta data
            bw.Write((Int32)DataType);
            bw.Write((Int32)Action);

            // call the virtual method
            WriteData(bw);

            bw.Dispose();
            var result = ms.ToArray();

            // handle length
            if (result.Length > byte.MaxValue)
            {
                throw new InvalidOperationException("Data message length is too big!");
            }

            result[0] = (byte)result.Length;

            return result;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlCommand"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data the command controls.</param>
        /// <param name="action">The action to perform, either <c>Start</c> or <c>Stop</c>.</param>
        public ControlCommand(DataType dataType, ControlCommandAction action)
        {
            DataType = dataType;
            Action = action;
        }

        /// <summary>
        /// If overwritten by a derived class, adds additional custom data to the raw representation of the command.
        /// </summary>
        /// <param name="bw">The binary writer used to write the raw represenation of the command.</param>
        protected virtual void WriteData(BinaryWriter bw)
        {
        }

        /// <summary>
        /// If overwritten by a derived class, reads back a raw command representation.
        /// </summary>
        /// <param name="br">The binary reader used to read the raw command representation.</param>
        internal virtual void ReadData(BinaryReader br)
        {
        }
    }
}