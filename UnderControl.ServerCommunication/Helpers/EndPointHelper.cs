using System;
using System.Net;

namespace UnderControl.Communication.Helpers
{
    /// <summary>
    /// Helper functions to parse string IP address data.
    /// </summary>
    internal static class EndPointHelper
    {
        /// <summary>
        /// Parses the given string representation of an IP address and returns a strongly typed <c>IPEndPoint</c>.
        /// </summary>
        /// <param name="address">The address to parse.</param>
        /// <param name="port">The port to use for the endpoint.</param>
        /// <returns>An <c>IPEndPoint</c> instance that contains the data passed in as arguments.</returns>
        public static IPEndPoint ParseEndPoint(string address, int port)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress))
            {
                throw new ArgumentException(string.Format("Cannot parse IP address from string '{0}'", address));
            }

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentException(string.Format("Invalid port given for IP end point: '{0}'", port));
            }

            var endPoint = new IPEndPoint(ipAddress, port);
            return endPoint;
        }
    }
}
