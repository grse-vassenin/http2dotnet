using Http2;
using Http2.Hpack;
using System;

namespace AGZCommon.Common.ConnectionBuilders
{
    public class StreamsServerConnectionBuilder
    {
        private IReadableByteStream _readableStream;

        private IWriteAndCloseableByteStream _writableStream;

        private Func<IStream, bool> _streamListener;

        public StreamsServerConnectionBuilder SetReadableStream(IReadableByteStream readableStream)
        {
            _readableStream = readableStream;
            return this;
        }

        public StreamsServerConnectionBuilder SetWritableStream(IWriteAndCloseableByteStream writeableStream)
        {
            _writableStream = writeableStream;
            return this;
        }

        public StreamsServerConnectionBuilder SetStreamListener(Func<IStream, bool> streamListener)
        {
            _streamListener = streamListener;
            return this;
        }

        public ConnectionWrapper Build()
        {
            var connectionConfiguration = new ConnectionConfigurationBuilder(true)
                    .UseStreamListener(_streamListener)
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
