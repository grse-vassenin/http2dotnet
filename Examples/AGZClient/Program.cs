using AGZCommon.Client;
using AGZCommon.Common;
using AGZCommon.Common.ConnectionBuilders;
using Http2;
using System.Threading.Tasks;


namespace AGZClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var clientConnectionWrapper = await new ConnectingClientConnectionBuilder()
                .SetHost("127.0.0.1")
                .SetPort(8889)
                .Build();
            if (!(clientConnectionWrapper?.IsValid ?? false))
                return;

            var client = new Client();
            //now we can communicate via connection
            await client.GetRequest(clientConnectionWrapper, "/client_get1");
            await client.GetRequest(clientConnectionWrapper, "/client_get2");
            await client.GetRequest(clientConnectionWrapper, "/client_get3");

            //and finally close the connection
            await clientConnectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError);

            //using same connection start listening for something
            var serverConnectionWrapper = new StreamsServerConnectionBuilder()
                .SetStreamHandler(new IncomingStreamHandler())
                .SetSslStream(clientConnectionWrapper.SslSteam)
                .Build();
            await serverConnectionWrapper.Connection.RemoteGoAwayReason;
            await serverConnectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError);
        }
    }
}
