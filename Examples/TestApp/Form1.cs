using System.IO;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using UplinkLib;

namespace TestApp
{
    public partial class UplinkForm : Form
    {
        private UplinkServer server;

        private UplinkClient client;

        public UplinkForm()
        {
            InitializeComponent();

            server = new UplinkServer()
            {
                Port = 7555,
                ServerCertificate = new X509Certificate2(ReadWholeStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TestApp.localhost.p12"))),
                SslProtocols = SslProtocols.Tls12,
                ClientCertificateRequired = true,
                IgnoreClientCertificateErrors = true
            };
            server.ProcessConnectionEvent += OnProcessConnectionEvent;
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

        private async void OnProcessConnectionEvent(ConnectionWrapper connectionWrapper)
        {
            if (client != null) //why meany connections?
                await client.Close();
            client = new UplinkClient()
            {
                ConnectionWrapper = connectionWrapper
            };
            sendRequest_btn.Enabled = true;
        }

        private void runServer_btn_Click(object sender, System.EventArgs e)
        {
            runServer_btn.Enabled = false;
            stopServer_btn.Enabled = true;
            var serverRunTask = server.Run();
        }

        private async void stopServer_btn_Click(object sender, System.EventArgs e)
        {
            if (client != null)
            {
                await client.Close();
                client = null;
                sendRequest_btn.Enabled = false;
            }
            server.Stop();
            runServer_btn.Enabled = true;
            stopServer_btn.Enabled = false;
        }

        private async void sendRequest_btn_Click(object sender, System.EventArgs e)
        {
            if (client == null ||
                requestPath_txt.Text.Length == 0 ||
                requestBody_txt.Text.Length == 0)
                return;
            replyBody_txt.Clear();
            replyBody_txt.Text = await client.SendSoapRequest(requestPath_txt.Text, requestBody_txt.Text);
        }
    }
}
