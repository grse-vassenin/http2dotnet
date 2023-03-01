using Http2;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CameraServer1
{
    internal class Server
    {
        public int Port { get; set; }

        public async Task Run()
        {
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();

            while (true)
            {
                Socket socket;
                try
                {
                    socket = await AcceptSocket(listener);
                }
                catch (Exception)
                {
                    return;
                }

                if (socket != null)
                    ProcessSocket(socket);
            }
        }

        private async Task<Socket> AcceptSocket(TcpListener listener)
        {
            var socket = await listener.AcceptSocketAsync();
            socket.NoDelay = true;
            return socket;
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
            var networkStream = new NetworkStream(socket, true);
            var sslStream = new SslStream(networkStream);
            var serverCertificate = new X509Certificate2(ReadWholeStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CameraServer1.localhost.p12")));
            try
            {
                //temporary do not check camera certificate
                await sslStream.AuthenticateAsServerAsync(serverCertificate, false, SslProtocols.Tls12, false);
            }
            catch (Exception)
            {
                return null;
            }
            return sslStream;
        }

        private async Task<bool> Revert(IReadableByteStream readStream, IWriteAndCloseableByteStream writeStream)
        {
            string fullData = string.Empty;
            while (true)
            {
                var buffer = new byte[1024];
                var readResult = await readStream.ReadAsync(new ArraySegment<byte>(buffer));
                if (readResult.EndOfStream)
                    return false;
                fullData += Encoding.ASCII.GetString(buffer, 0, readResult.BytesRead);
                if (fullData.Contains("\r\n\r\n"))
                {
                    fullData = fullData.Remove(fullData.Length - 4, 4);
                    var request = Http1Request.ParseFrom(fullData);
                    if (request.Method.ToLower() == "get" &&
                        request.Headers.ContainsKey("connection") &&
                        request.Headers["connection"].ToLower() == "upgrade" &&
                        request.Headers.ContainsKey("upgrade") &&
                        request.Headers["upgrade"].ToLower() == "h2c-reverse")
                        return true;
                    return false;
                }
                //else read more
            }
            return false;
        }

        private async Task ProcessSocket(Socket socket)
        {
            var sslStream = await Handshake(socket);
            if (sslStream == null)
                return;
            var wrappedStreams = sslStream.CreateStreams();
            if (!await Revert(wrappedStreams.ReadableStream, wrappedStreams.WriteableStream))
                return;

        }
    }
}
