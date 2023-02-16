using AGZCommon.Client;
using AGZCommon.Common;
using AGZCommon.Common.ConnectionBuilders;
using Http2;
using System;
using System.Threading;
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

            Console.WriteLine($"{DateTime.Now.Second} {DateTime.Now.Millisecond} Sending GoAway");

            //and finally close the connection
            await clientConnectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError, false);

            Console.WriteLine($"{DateTime.Now.Second} {DateTime.Now.Millisecond} GoAway finished");

            Console.WriteLine($"{DateTime.Now.Second} {DateTime.Now.Millisecond} Starting server via socket");

            Console.WriteLine($"{clientConnectionWrapper.SslSteam.CanRead}");
            Console.WriteLine($"{clientConnectionWrapper.SslSteam.CanWrite}");

            Thread.Sleep(1000);

            //using same connection start listening for something
            var serverConnectionWrapper = new StreamsServerConnectionBuilder()
                .SetStreamHandler(new IncomingStreamHandler())
                .SetSslStream(clientConnectionWrapper.SslSteam)
                .Build();
            Console.WriteLine($"{clientConnectionWrapper.SslSteam.CanRead}");
            Console.WriteLine($"{clientConnectionWrapper.SslSteam.CanWrite}");
            await serverConnectionWrapper.Connection.RemoteGoAwayReason;
            await serverConnectionWrapper.Connection.GoAwayAsync(ErrorCode.NoError);
        }
    }
}
