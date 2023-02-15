﻿using AGZCommon.Common;
using AGZCommon.Common.ConnectionBuilders;
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
                    .SetStreamHandler(new IncomingStreamHandler())
                    .SetCloseConnection(false)
                    .Build();
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
    }
}
