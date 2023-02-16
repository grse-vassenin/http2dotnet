using AGZCommon.Common;
using AGZCommon.Common.ConnectionBuilders;
using Http2;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AGZCommon.Server
{
    public class Server
    {
        public IPAddress Host { get; set; } = IPAddress.Any;

        public int Port { get; set; }

        private bool _run = true;

        private TcpListener _listener;

        public async Task Run()
        {
            _listener = new TcpListener(Host, Port);
            _listener.Start();

            while (_run)
            {
                Socket socket;
                try
                {
                    socket = await AcceptSocket(_listener);
                }
                catch (Exception)
                {
                    return;
                }

                var serverConnectionWrapper = await new ListeningServerConnectionBuilder()
                    .SetSocket(socket)
                    .SetStreamHandler(new IncomingStreamHandler())
                    .Build();
                var revertTask = RevertHandler(serverConnectionWrapper);
            }
        }

        public void Stop()
        {
            _run = false;
            _listener.Stop();

        }

        private async Task<Socket> AcceptSocket(TcpListener listener)
        {
            var socket = await listener.AcceptSocketAsync();
            socket.NoDelay = true;
            return socket;
        }

        private async Task RevertHandler(ConnectionWrapper serverConnectionWrapper)
        {
            await serverConnectionWrapper.Connection.RemoteGoAwayReason;
            await serverConnectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError);

            //time to create client connection using already existing streams and send some requests there
            var clientConnectionWrapper = new StreamsClientConnectionBuilder()
                .SetSslStream(serverConnectionWrapper.SslSteam)
                .Build();

            var client = new Client.Client();
            //communicate
            await client.GetRequest(clientConnectionWrapper, "/server_get1");
            await client.GetRequest(clientConnectionWrapper, "/server_get2");
            await client.GetRequest(clientConnectionWrapper, "/server_get3");

            //and finally close the connection
            await clientConnectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError, true);
        }
    }
}
