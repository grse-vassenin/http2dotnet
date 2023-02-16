using Http2;
using Http2.Hpack;
using System.Threading.Tasks;

namespace AGZCommon.Common.ConnectionBuilders
{
    public class StreamsServerConnectionBuilder : BaseStreamConnectionBuilder
    {
        private IIncomingStreamHandler _streamHandler;

        public StreamsServerConnectionBuilder SetStreamHandler(IIncomingStreamHandler streamHandler)
        {
            _streamHandler = streamHandler;
            return this;
        }

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

        private bool AcceptIncomingStream(IStream stream)
        {
            if (_streamHandler == null)
                return false;
            var handleStreamTask = Task.Run(() => _streamHandler.HandleStream(stream));
            return true;
        }

        private Connection CreateHttp2Connection()
        {
            var connectionConfiguration = new ConnectionConfigurationBuilder(true)
                .UseStreamListener(AcceptIncomingStream)
                .UseSettings(Settings.Default)
                .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                .Build();
            var wrappedStreams = _sslStream.CreateStreams();
            var connection = new Connection(connectionConfiguration, wrappedStreams.ReadableStream, wrappedStreams.WriteableStream);
            return connection;
        }
    }
}
