﻿using Http2;
using System.Net.Sockets;

namespace UplinkLib
{
    public class ConnectionWrapper
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public Connection Connection { get; set; }
    }
}
