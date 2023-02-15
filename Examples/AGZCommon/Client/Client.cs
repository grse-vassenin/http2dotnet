using AGZCommon.Common;
using Http2.Hpack;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AGZCommon.Client
{
    public class Client
    {
        public async Task GetRequest(ConnectionWrapper connectionWrapper, string path)
        {
            //create headers
            var hostValue = !string.IsNullOrEmpty(connectionWrapper.Host) ? connectionWrapper.Host : "0.0.0.0";
            var headers = new HeaderField[]
            {
                new HeaderField { Name = ":method", Value = "GET" },
                new HeaderField { Name = ":scheme", Value = "https" },
                new HeaderField { Name = ":path", Value = path },
                new HeaderField { Name = ":authority", Value = $"{hostValue}:{connectionWrapper.Port}" }
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
            Console.WriteLine(Encoding.UTF8.GetString(buffer));
            //read response trailers
            var responseTrailers = await stream.ReadTrailersAsync();
        }
    }
}
