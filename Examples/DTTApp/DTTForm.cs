using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Http2;
using Http2.Hpack;

namespace DTTApp
{
    public partial class DTTForm : Form
    {
        private TcpListener listener;
        private Connection connection;

        public DTTForm()
        {
            InitializeComponent();

            var runTask = Run();
        }

        private async Task Run()
        {
            listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();

            while (true)
            {
                var socket = await listener.AcceptSocketAsync();
                socket.NoDelay = true;
                log_txt.AppendText("Socket accepted\r\n");
                HandleSocket(socket);
            }
        }

        private void HandleSocket(Socket socket) 
        {
            var config =
                new ConnectionConfigurationBuilder(false)
                    .UseSettings(Settings.Default)
                    .UseHuffmanStrategy(HuffmanStrategy.IfSmaller)
                    .Build();
            var wrappedStreams = socket.CreateStreams();
            connection = new Connection(config, wrappedStreams.ReadableStream, wrappedStreams.WriteableStream);
        }

        private async void close_btn_Click(object sender, EventArgs e)
        {
            if (connection != null)
                await connection.GoAwayAsync(ErrorCode.NoError, true);
        }
    }
}
