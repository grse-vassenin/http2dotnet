using Http2;
using Http2.Hpack;

namespace AGZCommon.Common.ConnectionBuilders
{
    public class StreamsClientConnectionBuilder : BaseStreamConnectionBuilder
    {
        public override ConnectionWrapper Build()
        {
            var connection = CreateHttp2Connection();
            return new ConnectionWrapper()
            {
                IsValid = true,
                Connection = connection,
                SslSteam = _sslStream
            };
        }

        private Connection CreateHttp2Connection()
        {
            var connectionConfiguration = new ConnectionConfigurationBuilder(false)
                .UseSettings(Settings.Default)
                .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                .Build();
            var wrappedStreams = _sslStream.CreateStreams();
            return new Connection(connectionConfiguration, wrappedStreams.ReadableStream, wrappedStreams.WriteableStream);
        }
    }
}
