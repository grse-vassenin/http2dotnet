using Http2;
using System.Threading.Tasks;

namespace AGZCommon.Common
{
    public class ConnectionWrapper
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public bool IsValid { get; set; }

        public Connection Connection { get; set; }

        public IReadableByteStream ReadableStream { get; set; }

        public IWriteAndCloseableByteStream WritableStream { get; set; }

        public Task CompletionTask { get; set; }
    }
}
