using Http2;
using System.Threading.Tasks;

namespace AGZCommon.Server
{
    public interface IIncomingStreamHandler
    {
        Task HandleStream(IStream stream);
    }
}
