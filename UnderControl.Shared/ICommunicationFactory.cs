using UnderControl.Shared.Channels;
using UnderControl.Shared.Sockets;

namespace UnderControl.Shared
{
    /// <summary>
    /// An interface that describes a factory which can be used to create basic implementations of the communication layer.
    /// </summary>
    public interface ICommunicationFactory
    {
        /// <summary>
        /// Creates a TCP socket wrapper implementation.
        /// </summary>
        /// <returns>An implementation of the TCP socket wrapper interface.</returns>
        ITcpSocketWrapper CreateTcpSocket();

        /// <summary>
        /// Creates a UDP socket wrapper implementation.
        /// </summary>
        /// <returns>An implementation of the UDP socket wrapper interface.</returns>
        IUdpSocketWrapper CreateUdpSocket();

        /// <summary>
        /// Creates a data channel implementation.
        /// </summary>
        /// <returns>An implementation of the data channel interface.</returns>
        IDataChannel CreateDataChannel();

        /// <summary>
        /// Creates a control channel implementation.
        /// </summary>
        /// <returns>An implementation of the control channel interface.</returns>
        IControlChannel CreateControlChannel();
    }
}
