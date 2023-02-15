﻿using Http2;
using Http2.Hpack;
using System.Threading.Tasks;

namespace AGZCommon.Common.ConnectionBuilders
{
    public class StreamsServerConnectionBuilder
    {
        private IReadableByteStream _readableStream;

        private IWriteAndCloseableByteStream _writableStream;

        private IIncomingStreamHandler _streamHandler;

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

        public StreamsServerConnectionBuilder SetStreamHandler(IIncomingStreamHandler streamHandler)
        {
            _streamHandler = streamHandler;
            return this;
        }

        public ConnectionWrapper Build()
        {
            var connectionConfiguration = new ConnectionConfigurationBuilder(true)
                    .UseStreamListener(AcceptIncomingStream)
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

        private bool AcceptIncomingStream(IStream stream)
        {
            if (_streamHandler == null)
                return false;
            var handleStreamTask = Task.Run(() => _streamHandler.HandleStream(stream));
            return true;
        }

    }
}
