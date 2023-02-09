using Http2;
using System.Threading.Tasks;

namespace AGZServer
{
    internal static class IncomingStreamHandler
    {
        public static async Task HandleStream(IStream stream)
        {
            //do the work - read request and send the answer
        }
    }
}
