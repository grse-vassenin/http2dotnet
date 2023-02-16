using Http2;
using System.Net.Security;

namespace AGZCommon.Common
{
    public class ConnectionWrapper
    {
        public bool IsValid { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public Connection Connection { get; set; }

        public SslStream SslSteam { get; set; }
    }
}
