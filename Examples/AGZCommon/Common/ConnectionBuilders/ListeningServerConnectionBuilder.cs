﻿using Http2;
using Http2.Hpack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AGZCommon.Common.ConnectionBuilders
{
    public class ListeningServerConnectionBuilder
    {
        private Socket _socket;

        private Func<IStream, bool> _streamListener;

        private bool _closeConnection = true;

        public ListeningServerConnectionBuilder SetSocket(Socket socket)
        {
            _socket = socket;
            return this;
        }

        public ListeningServerConnectionBuilder SetStreamListener(Func<IStream, bool> streamListener)
        {
            _streamListener = streamListener;
            return this;
        }

        public ListeningServerConnectionBuilder SetCloseConnection(bool closeConnection)
        {
            _closeConnection = closeConnection;
            return this;
        }

        public async Task<ConnectionWrapper> Build()
        {
            var sslStream = await Handshake(_socket);
            var wrappedStreams = sslStream.CreateStreams();
            var upgradeReadStream = new UpgradeReadStream(wrappedStreams.ReadableStream);
            var upgrade = await Upgrade(upgradeReadStream, wrappedStreams.WriteableStream);
            var connectionConfiguration = new ConnectionConfigurationBuilder(true)
                .UseStreamListener(_streamListener)
                .UseSettings(Settings.Default)
                .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                .Build();
            var connection = CreateHttp2Connection(connectionConfiguration, upgradeReadStream, wrappedStreams.WriteableStream, upgrade);
            return new ConnectionWrapper()
            {
                IsValid = true,
                Connection = connection,
                ReadableStream = upgradeReadStream,
                WritableStream = wrappedStreams.WriteableStream
            };
        }

        private byte[] ReadWholeStream(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        private async Task<SslStream> Handshake(Socket socket)
        {
            var sslStream = new SslStream(new NetworkStream(socket, true));
            var serverCertificate = new X509Certificate2(ReadWholeStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("AGZCommon.localhost.p12")));
            await sslStream.AuthenticateAsServerAsync(serverCertificate, false, SslProtocols.Tls12, false);
            return sslStream;
        }

        private async Task<ServerUpgradeRequest> Upgrade(UpgradeReadStream upgradeReadStream, IWriteAndCloseableByteStream writableStream)
        {
            //read and check headers
            await upgradeReadStream.WaitForHttpHeader();
            var headerBytes = upgradeReadStream.HeaderBytes;
            var request = Http1Request.ParseFrom(Encoding.ASCII.GetString(headerBytes.Array, headerBytes.Offset, headerBytes.Count - 4));
            upgradeReadStream.ConsumeHttpHeader();
            if (request.Protocol != "HTTP/1.1")
                throw new Exception("Invalid upgrade request");
            string connectionHeader;
            string hostHeader;
            string upgradeHeader;
            string http2SettingsHeader;
            if (!request.Headers.TryGetValue("connection", out connectionHeader) ||
                !request.Headers.TryGetValue("host", out hostHeader) ||
                !request.Headers.TryGetValue("upgrade", out upgradeHeader) ||
                !request.Headers.TryGetValue("http2-settings", out http2SettingsHeader) ||
                upgradeHeader != "h2c" ||
                http2SettingsHeader.Length == 0)
                throw new Exception("Invalid upgrade request");
            var connParts = connectionHeader
                    .Split(new char[] { ',' })
                    .Select(p => p.Trim())
                    .ToArray();
            if (connParts.Length != 2 ||
                !connParts.Contains("Upgrade") ||
                !connParts.Contains("HTTP2-Settings"))
                throw new Exception("Invalid upgrade request");
            var headers = new List<HeaderField>
                {
                    new HeaderField {Name = ":method", Value = request.Method},
                    new HeaderField {Name = ":path", Value = request.Path},
                    new HeaderField { Name = ":scheme", Value = "https" }
                };
            foreach (var kvp in request.Headers)
            {
                // Skip Connection upgrade related headers
                if (kvp.Key == "connection" ||
                    kvp.Key == "upgrade" ||
                    kvp.Key == "http2-settings")
                    continue;
                headers.Add(new HeaderField
                {
                    Name = kvp.Key,
                    Value = kvp.Value,
                });
            }
            var upgradeBuilder = new ServerUpgradeRequestBuilder();
            upgradeBuilder.SetHeaders(headers);
            upgradeBuilder.SetHttp2Settings(http2SettingsHeader);
            var upgrade = upgradeBuilder.Build();
            if (!upgrade.IsValid)
                throw new Exception("Invalid upgrade");
            await writableStream.WriteAsync(new ArraySegment<byte>(UpgradeSuccessResponse));
            return upgrade;
        }

        private byte[] UpgradeSuccessResponse => Encoding.ASCII.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Connection: Upgrade\r\n" +
            $"Upgrade: h2c\r\n\r\n");

        private Connection CreateHttp2Connection(ConnectionConfiguration connectionConfiguration, IReadableByteStream readableStream, IWriteAndCloseableByteStream writableStream, ServerUpgradeRequest upgrade)
        {
            var connection = new Connection(connectionConfiguration, readableStream, writableStream,
                options: new Connection.Options
                {
                    ServerUpgradeRequest = upgrade
                });
            var completionTask = Task.Run(async () =>
            {
                await connection.RemoteGoAwayReason;
                await connection.GoAwayAsync(ErrorCode.NoError, _closeConnection);
            });
            return connection;
        }
    }
}
