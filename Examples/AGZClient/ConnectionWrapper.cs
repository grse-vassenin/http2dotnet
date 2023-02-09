using Http2;

namespace AGZClient
{
    internal class ConnectionWrapper
    {
        public bool IsValid { get; set; }

        public Connection Connection { get; set; }
    }
}
