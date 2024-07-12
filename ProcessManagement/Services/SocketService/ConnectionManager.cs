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
        public static IPEndPoint EndPoint { get { return new IPEndPoint(LocalIPAddress, Port); } }

        public static Socket CreateListener()
        {
            Socket? socket = null;
            try
            {
                // Create a TCP/IP socket.
                socket = CreateSocket();
                socket.Bind(EndPoint);
                socket.Listen(5);
            }
            catch (Exception)
            {
                throw;
            }

            return socket;
        }

        public static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
