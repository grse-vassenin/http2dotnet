using System.Threading.Tasks;

namespace CameraServer1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new Server()
            {
                Port = 7555
            };

            await server.Run();
        }

    }
}
