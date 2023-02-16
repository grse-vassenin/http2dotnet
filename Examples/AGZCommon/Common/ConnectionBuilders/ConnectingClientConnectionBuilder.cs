﻿using Http2;
using Http2.Hpack;
using System;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AGZCommon.Common.ConnectionBuilders
{
    public class ConnectingClientConnectionBuilder
    {
        private string _host;

        private int _port;

        public ConnectingClientConnectionBuilder SetHost(string host)
        {
            _host = host;
            return this;
        }

        public ConnectingClientConnectionBuilder SetPort(int port)
        {
            _port = port;
            return this;
        }

        public async Task<ConnectionWrapper> Build()
        {
            AvoidCertificateErrors();
            var socket = await EstablishTcpConnection();
            if (socket == null)
                return new ConnectionWrapper();
            var sslStream = await Handshake(socket);
            var wrappedStreams = sslStream.CreateStreams();
            var connectionConfiguration = CreateConnectionConfiguration();
            var clientUpgradeRequest = CreateClientUpgradeRequest(connectionConfiguration);
            var upgradeReadableStream = CreateUpgradeReadableStream(wrappedStreams.ReadableStream);
            if (!await DoTheUpgrade(clientUpgradeRequest, wrappedStreams.WriteableStream, upgradeReadableStream))
            {
                await wrappedStreams.WriteableStream.CloseAsync();
                return new ConnectionWrapper();
            }
            var connection = CreateHttp2Connection(connectionConfiguration, upgradeReadableStream, wrappedStreams.WriteableStream, clientUpgradeRequest);
            await FinalizeUpgrade(clientUpgradeRequest);
            return new ConnectionWrapper()
            {
                IsValid = true,
                Host = _host,
                Port = _port,
                Connection = connection,
                SslSteam = sslStream
            };
        }

        private void AvoidCertificateErrors()
        {
            //this should ignore server certificate validation error
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        private async Task<Socket> EstablishTcpConnection()
        {
            var tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(_host, _port);
            }
            catch (Exception)
            {
                return await Task.FromResult<Socket>(null);
            }
            tcpClient.Client.NoDelay = true;
            return tcpClient.Client;
        }

        private async Task<SslStream> Handshake(Socket socket)
        {
            var sslStream = new SslStream(new NetworkStream(socket, true), false, (sender, cert, chain, err) => true);
            await sslStream.AuthenticateAsClientAsync(_host);
            return sslStream;
        }

        private ConnectionConfiguration CreateConnectionConfiguration()
        {
            return new ConnectionConfigurationBuilder(false)
                .UseSettings(Settings.Default)
                .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                .Build();
        }

        private ClientUpgradeRequest CreateClientUpgradeRequest(ConnectionConfiguration connectionConfiguration)
        {
            return new ClientUpgradeRequestBuilder()
                .SetHttp2Settings(connectionConfiguration.Settings)
                .Build();
        }

        private UpgradeReadStream CreateUpgradeReadableStream(IReadableByteStream originalStream)
        {
            return new UpgradeReadStream(originalStream);
        }

        private async Task<bool> DoTheUpgrade(ClientUpgradeRequest clientUpgradeRequest, IWriteAndCloseableByteStream writableStream, UpgradeReadStream readableStream)
        {
            var upgradeHeader =
                "OPTIONS / HTTP/1.1\r\n" +
                "Host: " + _host + "\r\n" +
                "Connection: Upgrade, HTTP2-Settings\r\n" +
                "Upgrade: h2c\r\n" +
                "HTTP2-Settings: " + clientUpgradeRequest.Base64EncodedSettings + "\r\n\r\n";
            var encodedHeader = Encoding.ASCII.GetBytes(upgradeHeader);
            await writableStream.WriteAsync(new ArraySegment<byte>(encodedHeader));
            await readableStream.WaitForHttpHeader();
            var headerBytes = readableStream.HeaderBytes;
            var response = Http1Response.ParseFrom(Encoding.ASCII.GetString(headerBytes.Array, headerBytes.Offset, headerBytes.Count - 4));
            readableStream.ConsumeHttpHeader();
            if (response.StatusCode != "101")
                return false;
            if (!response.Headers.Any(hf => hf.Key == "connection" && hf.Value == "Upgrade") ||
                !response.Headers.Any(hf => hf.Key == "upgrade" && (hf.Value == "h2c" || hf.Value == "h2c-reverse")))
                return false;

            return true;
        }

        private Connection CreateHttp2Connection(ConnectionConfiguration connectionConfiguration, IReadableByteStream readableStream, IWriteAndCloseableByteStream writableStream, ClientUpgradeRequest clientUpgradeRequest)
        {
            return new Connection(connectionConfiguration, readableStream, writableStream, new Connection.Options()
            {
                ClientUpgradeRequest = clientUpgradeRequest
            });
        }

        private async Task FinalizeUpgrade(ClientUpgradeRequest clientUpgradeRequest)
        {
            var stream = await clientUpgradeRequest.UpgradeRequestStream;
            stream.Cancel();
        }
    }
}
