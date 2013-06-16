using UnderControl.Communication.Channels;
using UnderControl.Communication.Sockets;
using UnderControl.Shared;
using UnderControl.Shared.Channels;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication
{
    /// <summary>
    /// A default implementation of the <c>ICommunicationFactory</c> interface used for production.
    /// </summary>
    internal sealed class DefaultCommunicationFactory : ICommunicationFactory
    {
        /// <summary>
        /// Creates a TCP socket wrapper implementation.
        /// </summary>
        /// <returns>
        /// An implementation of the TCP socket wrapper interface.
        /// </returns>
        public ITcpSocketWrapper CreateTcpSocket()
        {
            return new TcpSocketWrapper();
        }

        /// <summary>
        /// Creates a UDP socket wrapper implementation.
        /// </summary>
        /// <returns>
        /// An implementation of the UDP socket wrapper interface.
        /// </returns>
        public IUdpSocketWrapper CreateUdpSocket()
        {
            return new UdpSocketWrapper();
        }

        /// <summary>
        /// Creates a data channel implementation.
        /// </summary>
        /// <returns>
        /// An implementation of the data channel interface.
        /// </returns>
        public IDataChannel CreateDataChannel()
        {
            return new DataChannel(this);
        }

        /// <summary>
        /// Creates a control channel implementation.
        /// </summary>
        /// <returns>
        /// An implementation of the control channel interface.
        /// </returns>
        public IControlChannel CreateControlChannel()
        {
            return new ControlChannel(this);
        }
    }
}
