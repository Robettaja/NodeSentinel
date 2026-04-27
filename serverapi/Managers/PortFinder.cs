using System;
using System.Net;
using System.Net.Sockets;

namespace serverapi.Managers
{
    public static class PortFinder
    {
        public static int GetNextAvailablePort(int startingPort, int maxPort = 65535)
        {
            for (int port = startingPort; port <= maxPort; port++)
            {
                if (IsPortAvailable(port))
                    return port;
            }

            throw new InvalidOperationException("No available ports found in range.");
        }

        private static bool IsPortAvailable(int port)
        {
            try
            {
                using var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
