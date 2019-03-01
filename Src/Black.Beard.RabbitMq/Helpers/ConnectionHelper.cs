using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Bb.Helpers
{

    internal static class ConnectionHelper
    {

        public static string LocalhostHandling(this string s)
        {
            if (s.ToLower() == "localhost")
                return GetDefaultLocalIp();
            return s;
        }

        /// <summary>
        /// Select the IP of the first non-loopback interface with a valid IPv4 gateway.
        /// </summary>
        private static string GetDefaultLocalIp()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.GetIPProperties()?.GatewayAddresses?.Count > 0)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .Where(a => a?.Address != null && a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address)
                .FirstOrDefault()?.ToString() ?? "localhost";
        }
    }


}
