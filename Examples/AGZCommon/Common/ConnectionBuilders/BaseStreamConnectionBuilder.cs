using System.Net.Security;

namespace AGZCommon.Common.ConnectionBuilders
{
    public abstract class BaseStreamConnectionBuilder
    {
        protected SslStream _sslStream;

        public BaseStreamConnectionBuilder SetSslStream(SslStream sslStream)
        {
            _sslStream = sslStream;
            return this;
        }

        public abstract ConnectionWrapper Build();
    }
}
