using Http2;
using System.Threading.Tasks;

namespace AGZCommon.Server
{
    public interface IIncomingStreamHandler
    {
        bool DoTheWork { get; set; }

        Task HandleStream(IStream stream);
    }
}
