using AGZCommon.Common;
using Http2;
using Http2.Hpack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AGZCommon.Server
{
    public class Server
    {
        public IPAddress Host { get; set; } = IPAddress.Any;

        public int Port { get; set; }

        public Server()
        {
        }

        public delegate Task HandleStreamDelegate(IStream stream);

        public HandleStreamDelegate HandleStreamHandler { get; set; }

        public async Task Run()
        {
            var listener = new TcpListener(Host, Port);
            listener.Start();

            var config = new ConnectionConfigurationBuilder(true)
                    .UseStreamListener(AcceptIncomingStream)
                    .UseSettings(Settings.Default)
                    .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                    .Build();

            while (true)
            {
                var socket = await AcceptSocket(listener);
                var sslStream = await Handshake(socket);
                var wrappedStreams = sslStream.CreateStreams();
                await HandleConnection(config, wrappedStreams.ReadableStream, wrappedStreams.WriteableStream);
            }
        }

        private bool AcceptIncomingStream(IStream stream)
        {
            var handleStreamTask = Task.Run(() => HandleStreamHandler?.Invoke(stream));
            return true;
        }

        private async Task<Socket> AcceptSocket(TcpListener listener)
        {
            var socket = await listener.AcceptSocketAsync();
            socket.NoDelay = true;
            return socket;
        }

        private static byte[] ReadWholeStream(Stream stream)
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

        private byte[] UpgradeSuccessResponse(string upgradeType) => Encoding.ASCII.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Connection: Upgrade\r\n" +
            $"Upgrade: {upgradeType}\r\n\r\n");

        private async Task HandleConnection(ConnectionConfiguration connnectionConfiguration, IReadableByteStream readableStream, IWriteAndCloseableByteStream writableStream)
        {
            //here we want to see that connection is done via http1.1 and first request is upgrade request and do the upgrade
            var upgradeReadStream = new UpgradeReadStream(readableStream);
            ServerUpgradeRequest upgrade = null;
            try
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
                var upgradeType = "h2c";
                foreach (var kvp in request.Headers)
                {
                    // Skip Connection upgrade related headers
                    if (kvp.Key == "connection" ||
                        kvp.Key == "upgrade" ||
                        kvp.Key == "http2-settings")
                        continue;
                    if (kvp.Key == "upgrade")
                        upgradeType = kvp.Value;
                    headers.Add(new HeaderField
                    {
                        Name = kvp.Key,
                        Value = kvp.Value,
                    });
                }
                var upgradeBuilder = new ServerUpgradeRequestBuilder();
                upgradeBuilder.SetHeaders(headers);
                upgradeBuilder.SetHttp2Settings(http2SettingsHeader);
                upgrade = upgradeBuilder.Build();
                if (!upgrade.IsValid)
                    throw new Exception("Invalid upgrade");
                await writableStream.WriteAsync(new ArraySegment<byte>(UpgradeSuccessResponse(upgradeType)));
            }
            catch (Exception)
            {
                await writableStream.CloseAsync();
                return;
            }
            //create a connection
            var connection = new Connection(connnectionConfiguration, upgradeReadStream, writableStream,
                options: new Connection.Options
                {
                    ServerUpgradeRequest = upgrade
                });
            //wait till connection is closed
            // Close the connection if we get a GoAway from the client
            var completionTask = Task.Run(async () =>
            {
                await connection.RemoteGoAwayReason;
                await connection.GoAwayAsync(ErrorCode.NoError, true);
            });
        }
    }
}
