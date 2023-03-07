using System.Threading.Tasks;
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
            if (client != null)
            //why meany connections?
            {
                Task closeTaks;
                closeTaks = client.Close();
            }
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
                Task closeTaks;
                closeTaks = client.Close();
                client = null;
            }
            server.Stop();
        }

        private void sendRequest_btn_Click(object sender, System.EventArgs e)
        {
            var soapRequest =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:tds=\"http://www.onvif.org/ver10/device/wsdl\" xmlns:tt=\"http://www.onvif.org/ver10/schema\">\r\n" +
                "<s:Body>\r\n" +
                "<tds:GetServices>\r\n" +
                "<tds:IncludeCapability>true</tds:IncludeCapability>\r\n" +
                "</tds:GetServices>\r\n" +
                "</s:Body>\r\n" +
                "</s:Envelope>";
            replyBody_txt.Clear();
            if (client == null ||
                requestPath_txt.Text.Length == 0 ||
                requestBody_txt.Text.Length == 0)
                return;
            //var replyTask = client.SendSoapRequest(requestPath_txt.Text, requestBody_txt.Text);
            var replyTask = client.SendSoapRequest(requestPath_txt.Text, soapRequest);
            var replyAwaiter = replyTask.GetAwaiter();
            replyAwaiter.OnCompleted(() =>
            {
                replyBody_txt.Text = replyAwaiter.GetResult();
            });
        }
    }
}
