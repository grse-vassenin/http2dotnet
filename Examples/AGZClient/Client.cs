using System.Threading.Tasks;

namespace AGZClient
{
    internal class Client
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public async Task<ConnectionWrapper> EstablishConnection()
        {
            return await new ConnectionBuilder()
                .SetHost(Host)
                .SetPort(Port)
                .Build();
        }
    }
}
