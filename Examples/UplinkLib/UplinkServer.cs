﻿using Http2;
using Http2.Hpack;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace UplinkLib
{
    public class UplinkServer
    {
        public int Port { get; set; }

        public X509Certificate ServerCertificate { get; set; }

        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls12;

        public bool ClientCertificateRequired { get; set; }

        public bool IgnoreClientCertificateErrors { get; set; }

        public delegate void ProcessConnectionDelegate(ConnectionWrapper connectionWrapper);

        public event ProcessConnectionDelegate ProcessConnectionEvent;

        private bool run;

        private TcpListener listener = null;

        public async Task Run()
        {
            if (listener != null)
                return;
            try
            {
                listener = new TcpListener(IPAddress.Any, Port);
                listener.Start();

                run = true;

                while (run)
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
                        await ProcessSocket(socket);
                }
            }
            catch (Exception)
            {

                //ignore
            }
        }

        public void Stop()
        {
            try
            {
                run = false;
                listener?.Stop();
            }
            catch (Exception)
            {
                //ignore
            }
            finally
            {
                listener = null;
            }
        }

        private async Task<Socket> AcceptSocket(TcpListener listener)
        {
            try
            {
                var socket = await listener.AcceptSocketAsync();
                socket.NoDelay = true;
                return socket;
            }
            catch (Exception)
            {
                //ignore it
            }
            return null;

        }

        private async Task<SslStream> Handshake(Socket socket)
        {
            var networkStream = new NetworkStream(socket);
            var sslStream = IgnoreClientCertificateErrors
                ? new SslStream(networkStream, false, (sender, certificate, chain, errors) => true)
                : new SslStream(networkStream, false);
            try
            {
                //temporary do not check camera certificate
                await sslStream.AuthenticateAsServerAsync(ServerCertificate, ClientCertificateRequired, SslProtocols, false);
            }
            catch (Exception)
            {
                if (!IgnoreClientCertificateErrors)
                    throw;
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

        private void EmitProcessConnectionEvent(ConnectionWrapper connectionWrapper)
        {
            ProcessConnectionEvent?.Invoke(connectionWrapper);
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
            var connectionWrapper = new ConnectionWrapper()
            {
                Host = string.Empty,
                Port = Port,
                Connection = connection
            };
            EmitProcessConnectionEvent(connectionWrapper);
        }
    }
}
