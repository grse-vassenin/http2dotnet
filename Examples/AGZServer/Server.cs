using Http2;
using Http2.Hpack;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AGZServer
{
    internal class Server
    {
        public IPAddress Host { get; set; } = IPAddress.Any;

        public int Port { get; set; }

        public Server()
        {
        }

        public async Task Run()
        {
            var listener = new TcpListener(Host, Port);
            listener.Start();

            var config = new ConnectionConfigurationBuilder(true)
                    .UseStreamListener((stream) =>
                    {
                        IncomingStreamHandler.HandleStream(stream);
                        return true;
                    })
                    .UseSettings(Settings.Default)
                    .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                    .Build();

            while (true)
            {
                var socket = await AcceptSocket(listener);
                var sslStream = await Handshake(socket);
                var wrappedStreams = sslStream.CreateStreams();
                await HandleConnection(wrappedStreams.ReadableStream, wrappedStreams.WriteableStream);
            }
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
            var serverCertificate = new X509Certificate2(ReadWholeStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("AGZServer.localhost.p12")));
            await sslStream.AuthenticateAsServerAsync(serverCertificate, false, SslProtocols.Tls12, false);
            return sslStream;
        }

        private async Task HandleConnection(IReadableByteStream readableStream, IWriteAndCloseableByteStream writableStream)
        {
            //here we want to see that connection is done via http1.1 and first request is upgrade request and do the upgrade
        }
    }
}
