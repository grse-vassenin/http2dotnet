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
                CertificatePath = "UplinkLib.localhost.p12"
            };
            server.ProcessConnectionEvent += OnProcessConnectionEvent;
        }

        private void OnProcessConnectionEvent(ConnectionWrapper connectionWrapper)
        {
            /*if (client != null)
            {
                //why meany connections?
                var closeTask = client.Close();
                closeTask.Wait();
            }*/
            client = new UplinkClient()
            {
                ConnectionWrapper = connectionWrapper
            };
            sendRequest_btn.Enabled = true;
        }

        private void runServer_btn_Click(object sender, System.EventArgs e)
        {
            var serverRunTask = server.Run();
            runServer_btn.Enabled = false;
            stopServer_btn.Enabled = true;
        }

        private void stopServer_btn_Click(object sender, System.EventArgs e)
        {
            runServer_btn.Enabled = false;
            stopServer_btn.Enabled = false;
            if (client != null)
            {
                sendRequest_btn.Enabled = false;
                client.Close().Wait();
                client = null;
            }
            server.Stop();
        }

        private void sendRequest_btn_Click(object sender, System.EventArgs e)
        {
            replyBody_txt.Clear();
            if (client == null ||
                requestPath_txt.Text.Length == 0 ||
                requestBody_txt.Text.Length == 0)
                return;
            var reply = client.SendSoapRequest(requestPath_txt.Text, requestBody_txt.Text).Result;
            replyBody_txt.Text = reply;
        }
    }
}
