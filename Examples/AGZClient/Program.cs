using AGZCommon.Client;
using System.Threading.Tasks;


namespace AGZClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //this should ignore server certificate validation error
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var client = new Client();
            var connectionWrapper = await client.EstablishConnection("127.0.0.1", 8889);
            if (!(connectionWrapper?.IsValid ?? false))
                return;
            //now we can communicate via connection
            await client.GetRequest(connectionWrapper, "/get1");
            await client.GetRequest(connectionWrapper, "/get2");
            await client.GetRequest(connectionWrapper, "/get3");

            //and finally close the connection
            //await connectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError, true);
            //return;

            //or let's switch sides
            /*await connectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError, false);
            var server = new Server()
            {
                Host = IPAddress.Any,
                Port = 8889,
                HandleStreamHandler = IncomingStreamHandler.HandleStream
            };*/
        }
    }
}
