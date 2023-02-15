using AGZCommon.Client;
using AGZCommon.Common.ConnectionBuilders;
using System.Threading.Tasks;


namespace AGZClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //this should ignore server certificate validation error
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var connectionWrapper = await new ConnectingClientConnectionBuilder()
                .SetHost("127.0.0.1")
                .SetPort(8889)
                .Build();
            if (!(connectionWrapper?.IsValid ?? false))
                return;

            var client = new Client();
            //now we can communicate via connection
            await client.GetRequest(connectionWrapper, "/get1");
            await client.GetRequest(connectionWrapper, "/get2");
            await client.GetRequest(connectionWrapper, "/get3");

            //and finally close the connection
            //await connectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError, true);
        }
    }
}
