using Http2;

namespace AGZCommon.Client
{
    public class ConnectionWrapper
    {
        public bool IsValid { get; set; }

        public Connection Connection { get; set; }
    }
}
