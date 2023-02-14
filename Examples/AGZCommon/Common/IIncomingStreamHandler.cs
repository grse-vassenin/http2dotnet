using Http2;
using System.Threading.Tasks;

namespace AGZCommon.Common
{
    public interface IIncomingStreamHandler
    {
        Task HandleStream(IStream stream);
    }
}
