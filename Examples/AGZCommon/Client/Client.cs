using Http2.Hpack;
using System;
using System.Threading.Tasks;

namespace AGZCommon.Client
{
    public class Client
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public async Task<ConnectionWrapper> EstablishConnection()
        {
            return await new ConnectionBuilder()
                .SetHost(Host)
                .SetPort(Port)
                .Build();
        }

        public async Task GetRequest(ConnectionWrapper connectionWrapper, string path)
        {
            //create headers
            var headers = new HeaderField[]
            {
                new HeaderField { Name = ":method", Value = "GET" },
                new HeaderField { Name = ":scheme", Value = "https" },
                new HeaderField { Name = ":path", Value = path },
                new HeaderField { Name = ":authority", Value = $"{Host}:{Port}" }
            };
            //crate a stream which will also write (send) all headers
            //you can use this stream to send body after headers if you want to do POST
            var stream = await connectionWrapper.Connection.CreateStreamAsync(headers, true);
            //read response headers
            var responseHeaders = await stream.ReadHeadersAsync();
            //read response body
            byte[] buffer = new byte[8192];
            while (true)
            {
                var res = await stream.ReadAsync(new ArraySegment<byte>(buffer));
                if (res.EndOfStream)
                    break;
            }
            //read response trailers
            var responseTrailers = await stream.ReadTrailersAsync();
        }
    }
}
