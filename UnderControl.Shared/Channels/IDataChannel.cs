using System;
using UnderControl.Shared.Data;

namespace UnderControl.Shared.Channels
{
    /// <summary>
    /// An interface that describes a specialized channel to send acquired data from the client to the server.
    /// </summary>
    public interface IDataChannel : IChannel
    {
        /// <summary>
        /// Gets or sets the minimum number of milliseconds between messages.
        /// Messages are delayed by the amount given here if necessary.
        /// </summary>
        /// <value>
        /// The minimum number of milliseconds between messages.
        /// </value>
        int MinMillisecondsBetweenMessages
        {
            get;
            set;
        }

        /// <summary>
        /// Occurs when a data message was successfully received, on the server side.
        /// </summary>
        event EventHandler<DataMessageEventArgs> DataMessageReceived;

        /// <summary>
        /// Sends the specified data message to the server.
        /// </summary>
        /// <param name="data">The data message to send.</param>
        void Send(IDataMessage data);

        /// <summary>
        /// Purges a specific type of data messages from the send queue.
        /// </summary>
        /// <param name="dataType">The type of the data to purge from the send queue.</param>
        void PurgeFromQueue(DataType dataType);
    }
}