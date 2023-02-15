using Http2;
using Http2.Hpack;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGZCommon.Common
{
    public class IncomingStreamHandler : IIncomingStreamHandler
    {
        public ConnectionWrapper ConnectionWrapper { get; set; }

        public async Task HandleStream(IStream stream)
        {
            //do the work - read request and send the answer
            try
            {
                //read headers
                var headers = await stream.ReadHeadersAsync();
                var method = headers.First(h => h.Name == ":method").Value;
                var path = headers.First(h => h.Name == ":path").Value;
                //read body
                var buf = new byte[2048];
                while (true)
                {
                    var readResult = await stream.ReadAsync(new ArraySegment<byte>(buf));
                    if (readResult.EndOfStream)
                        break;
                }
                //send response headers
                var responseHeaders = new HeaderField[] {
                    new HeaderField { Name = ":status", Value = "200" },
                    new HeaderField { Name = "content-type", Value = "text/html" },
                };
                await stream.WriteHeadersAsync(responseHeaders, false);
                //send response body
                var responseString = $"<html>{method} + {path}</html>";
                await stream.WriteAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(responseString)), true);
            }
            catch (Exception)
            {
                stream.Cancel();
            }
        }
    }
}
