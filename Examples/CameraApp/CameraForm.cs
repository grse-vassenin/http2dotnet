using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Http2;
using Http2.Hpack;

namespace CameraApp
{
    public partial class CameraForm : Form
    {
        private Socket socket;
        private Connection connection;

        public CameraForm()
        {
            InitializeComponent();

        }

        static async Task HandleIncomingStream(IStream stream)
        {

        }

        static bool AcceptIncomingStream(IStream stream)
        {
            var task = HandleIncomingStream(stream);
            return true;
        }

        private async void connect_btn_Click(object sender, EventArgs e)
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("127.0.0.1", 8888);
            log_txt.AppendText("Connected\r\n");
            socket = tcpClient.Client;
            socket.NoDelay = true;

            var wrappedStreams = socket.CreateStreams();
            var config = new ConnectionConfigurationBuilder(true)
                    .UseStreamListener(AcceptIncomingStream)
                    .UseSettings(Settings.Default)
                    .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                    .Build();
            connection = new Connection(config, wrappedStreams.ReadableStream, wrappedStreams.WriteableStream);
            var remoteGoAwayTask = connection.RemoteGoAwayReason;
            var closeWhenRemoteGoAway = Task.Run(async () =>
            {
                await remoteGoAwayTask;
                await connection.GoAwayAsync(ErrorCode.NoError, true);
            });
        }
    }
}
