using AGZCommon.Server;
using System.Threading.Tasks;

namespace AGZServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new Server()
            {
                Port = 8889,
                IncominStreamHandler = new IncomingStreamHandler(),
                CloseConnection = false
            };
            await server.Run();
        }
    }
}
