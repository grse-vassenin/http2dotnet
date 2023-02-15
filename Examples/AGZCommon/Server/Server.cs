using AGZCommon.Common;
using AGZCommon.Common.ConnectionBuilders;
using Http2;
using Http2.Hpack;
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

        public bool CloseConnection { get; set; }

        private bool _run = true;

        private TcpListener _listener;

        public Server()
        {
        }

        public IIncomingStreamHandler IncominStreamHandler { get; set; }

        public async Task Run()
        {
            _listener = new TcpListener(Host, Port);
            _listener.Start();

            var connectionConfiguration = new ConnectionConfigurationBuilder(true)
                    .UseStreamListener(AcceptIncomingStream)
                    .UseSettings(Settings.Default)
                    .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                    .Build();

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
                var connectionWrapper = new ListeningServerConnectionBuilder()
                    .SetSocket(socket)
                    .SetStreamListener(AcceptIncomingStream)
                    .SetCloseConnection(false)
                    .Build();
            }
        }

        public void Stop()
        {
            _run = false;
            _listener.Stop();

        }

        private bool AcceptIncomingStream(IStream stream)
        {
            var handleStreamTask = Task.Run(() => IncominStreamHandler?.HandleStream(stream));
            return true;
        }

        private async Task<Socket> AcceptSocket(TcpListener listener)
        {
            var socket = await listener.AcceptSocketAsync();
            socket.NoDelay = true;
            return socket;
        }
    }
}
