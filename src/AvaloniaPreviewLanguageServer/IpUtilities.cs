using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace AvaloniaPreviewLanguageServer
{
    internal static class IpUtilities
    {
        private const ushort MinPort = 1;
        private const ushort MaxPort = UInt16.MaxValue;

        private static readonly Random Rnd = new();

        public static int GetAvailablePort()
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var usedPorts = Enumerable.Empty<int>()
                .Concat(ipProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint.Port))
                .Concat(ipProperties.GetActiveTcpListeners().Select(l => l.Port))
                .Concat(ipProperties.GetActiveUdpListeners().Select(l => l.Port))
                .ToHashSet();

            while (true)
            {
                var port = Rnd.Next(MinPort, MaxPort);
                if (!usedPorts.Contains(port))
                    return port;
            }
        }
    }
}
