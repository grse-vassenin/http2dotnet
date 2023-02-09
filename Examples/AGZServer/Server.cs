using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AGZServer
{
    internal class Server
    {
        public IPAddress Host { get; set; } = IPAddress.Any;

        public int Port { get; set; }

        private TcpListener listener;

        public Server()
        {
            listener = new TcpListener(Host, Port);
            listener.Start();
        }

        public async Task Run()
        {

        }
    }
}
