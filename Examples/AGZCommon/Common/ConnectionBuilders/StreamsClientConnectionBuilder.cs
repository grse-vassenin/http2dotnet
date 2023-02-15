using Http2;
using Http2.Hpack;

namespace AGZCommon.Common.ConnectionBuilders
{
    public class StreamsClientConnectionBuilder
    {
        private IReadableByteStream _readableStream;

        private IWriteAndCloseableByteStream _writableStream;

        public StreamsClientConnectionBuilder SetReadableStream(IReadableByteStream readableStream)
        {
            _readableStream = readableStream;
            return this;
        }

        public StreamsClientConnectionBuilder SetWritableStream(IWriteAndCloseableByteStream writeableStream)
        {
            _writableStream = writeableStream;
            return this;
        }

        public ConnectionWrapper Build()
        {
            var connectionConfiguration = new ConnectionConfigurationBuilder(false)
                .UseSettings(Settings.Default)
                .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                .Build();
            var connection = new Connection(connectionConfiguration, _readableStream, _writableStream);
            return new ConnectionWrapper()
            {
                IsValid = true,
                Connection = connection,
                ReadableStream = _readableStream,
                WritableStream = _writableStream
            };
        }
    }
}
