using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ProcessManagement.Services.SocketService
{
    public class ConnectionManager
    {
        //public static IPAddress LocalIPAddress { get { return IPAddress.Parse("192.168.1.3"); } }
        public static IPAddress LocalIPAddress { get { return IPAddress.Any; } }
        public static int Port { get { return 8998; } }
        //public static IPEndPoint EndPoint { get { return new IPEndPoint(LocalIPAddress, Port); } }

        public static Socket CreateListener()
        {
            var EndPoint = FindAvailableEndpoint(LocalIPAddress, Port);

            Socket? socket = CreateSocket();

            try
            {
                // Create a TCP/IP socket.

                socket.Bind(EndPoint);

                socket.Listen(5);
            }
            catch (Exception)
            {
                socket.Dispose();

                throw;
            }

            return socket;
        }

        public static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private static IPEndPoint FindAvailableEndpoint(IPAddress ipAddress, int preferredPort, int maxPort = 65535)
        {
            // Try the preferred port first
            var endPoint = new IPEndPoint(ipAddress, preferredPort);

            if (IsEndpointAvailable(endPoint))
            {
                return endPoint;
            }

            // If preferred port is not available, scan for the next available port
            for (int port = preferredPort + 1; port <= preferredPort + 5; port++)
            {
                endPoint = new IPEndPoint(ipAddress, port);

                if (IsEndpointAvailable(endPoint))
                {
                    return endPoint;
                }
            }

            throw new InvalidOperationException($"No available ports found between {preferredPort} and {maxPort}");
        }

        private static bool IsEndpointAvailable(IPEndPoint endPoint)
        {
            try
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);
                socket.Listen(1);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
