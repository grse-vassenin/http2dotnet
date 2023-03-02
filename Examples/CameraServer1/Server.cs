using Http2;
using Http2.Hpack;
using System;
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

        public async Task<string> SendSoapRequest(Connection connection, string host, int port, string path, string soapRequest)
        {
            Console.WriteLine("SendSoapRequest");
            Console.WriteLine("Request:");
            Console.WriteLine(soapRequest);
            var soapRequestBytes = Encoding.UTF8.GetBytes(soapRequest);
            //create headers
            var hostValue = !string.IsNullOrEmpty(host) ? host : "0.0.0.0";
            var headers = new HeaderField[]
            {
                new HeaderField { Name = ":method", Value = "POST" },
                new HeaderField { Name = ":scheme", Value = "https" },
                new HeaderField { Name = ":path", Value = path },
                new HeaderField { Name = ":authority", Value = $"{hostValue}:{port}" },
                new HeaderField { Name = "content-type", Value = "application/soap+xml; charset=utf-8" },
                new HeaderField { Name = "content-length", Value = $"{soapRequestBytes.Length}" }
            };
            //crate a stream which will also write (send) all headers
            var stream = await connection.CreateStreamAsync(headers);
            //now send request content
            await stream.WriteAsync(new ArraySegment<byte>(soapRequestBytes));
            //read response headers
            var responseHeaders = await stream.ReadHeadersAsync();
            if (!responseHeaders.Any(h => h.Name == ":status") ||
                responseHeaders.First(h => h.Name == ":status").Value != "200" ||
                !responseHeaders.Any(h => h.Name == "content-length"))
                return null;
            var contentLength = int.Parse(responseHeaders
                .First(h => h.Name == "content-length")
                .Value);
            //read response body
            var fullBuffer = new byte[contentLength];
            int offset = 0;
            while (true)
            {
                var readResult = await stream.ReadAsync(new ArraySegment<byte>(fullBuffer, offset, contentLength - offset));
                if (readResult.EndOfStream)
                    break;
                offset += readResult.BytesRead;
                if (offset >= contentLength)
                    break;
            }
            var soapReply = Encoding.UTF8.GetString(fullBuffer);
            Console.WriteLine("Reply:");
            Console.WriteLine(soapReply);
            stream.Cancel();
            return soapReply;

        }

        private async Task<Socket> AcceptSocket(TcpListener listener)
        {
            var socket = await listener.AcceptSocketAsync();
            socket.NoDelay = true;
            Console.WriteLine("Socket opened");
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
            var networkStream = new NetworkStream(socket, false);
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

        private byte[] UpgradeSuccessResponse => Encoding.ASCII.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Connection: Upgrade\r\n" +
            "Upgrade: h2c-reverse\r\n\r\n");

        private async Task<bool> Revert(IReadableByteStream readStream, IWriteAndCloseableByteStream writeStream)
        {
            string fullData = string.Empty;
            //read request
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
                        break;
                    return false;
                }
                //else read more
            }
            //reply with 101
            await writeStream.WriteAsync(new ArraySegment<byte>(UpgradeSuccessResponse));
            return true;
        }

        private async Task ProcessSocket(Socket socket)
        {
            var sslStream = await Handshake(socket);
            if (sslStream == null)
                return;
            var wrappedStreams = sslStream.CreateStreams();
            if (!await Revert(wrappedStreams.ReadableStream, wrappedStreams.WriteableStream))
                return;
            var connectionConfiguration = new ConnectionConfigurationBuilder(false)
                .UseSettings(Settings.Default)
                .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                .Build();
            var connection = new Connection(connectionConfiguration, wrappedStreams.ReadableStream, wrappedStreams.WriteableStream);

            var soapRequest =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:tds=\"http://www.onvif.org/ver10/device/wsdl\" xmlns:tt=\"http://www.onvif.org/ver10/schema\">\r\n" +
                "<s:Body>\r\n" +
                "<tds:GetServices>\r\n" +
                "<tds:IncludeCapability>true</tds:IncludeCapability>\r\n" +
                "</tds:GetServices>\r\n" +
                "</s:Body>\r\n" +
                "</s:Envelope>";

            await SendSoapRequest(connection, "", Port, "/onvif/uplink_service", soapRequest);

            await SendSoapRequest(connection, "", Port, "/onvif/uplink_service", soapRequest);

            await connection.CloseNow();

            socket.Close();
        }
    }
}
