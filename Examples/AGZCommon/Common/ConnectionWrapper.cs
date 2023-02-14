using Http2;

namespace AGZCommon.Common
{
    public class ConnectionWrapper
    {
        public bool IsValid { get; set; }

        public Connection Connection { get; set; }

        public IReadableByteStream ReadableStream { get; set; }

        public IWriteAndCloseableByteStream WritableStream { get; set; }
    }
}
