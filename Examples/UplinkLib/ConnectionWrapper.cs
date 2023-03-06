using Http2;
using System.Net.Sockets;

namespace UplinkLib
{
    public class ConnectionWrapper
    {
        public Socket Socket { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public Connection Connection { get; set; }
    }
}
