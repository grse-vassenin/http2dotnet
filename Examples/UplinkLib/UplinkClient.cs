using Http2.Hpack;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UplinkLib
{
    public class UplinkClient
    {
        public ConnectionWrapper ConnectionWrapper { get; set; }

        public async Task<string> SendSoapRequest(string path, string soapRequest)
        {
            var soapRequestBytes = Encoding.UTF8.GetBytes(soapRequest);
            //create headers
            var hostValue = !string.IsNullOrEmpty(ConnectionWrapper.Host) ? ConnectionWrapper.Host : "0.0.0.0";
            var headers = new HeaderField[]
            {
                new HeaderField { Name = ":method", Value = "POST" },
                new HeaderField { Name = ":scheme", Value = "https" },
                new HeaderField { Name = ":path", Value = path },
                new HeaderField { Name = ":authority", Value = $"{hostValue}:{ConnectionWrapper.Port}" },
                new HeaderField { Name = "content-type", Value = "application/soap+xml; charset=utf-8" },
                new HeaderField { Name = "content-length", Value = $"{soapRequestBytes.Length}" }
            };
            //crate a stream which will also write (send) all headers
            var stream = await ConnectionWrapper.Connection.CreateStreamAsync(headers);
            //now send request content
            await stream.WriteAsync(new ArraySegment<byte>(soapRequestBytes));
            //read response headers
            var responseHeaders = await stream.ReadHeadersAsync();
            if (!responseHeaders.Any(h => h.Name == ":status") ||
                responseHeaders.First(h => h.Name == ":status").Value != "200" ||
                !responseHeaders.Any(h => h.Name == "content-length"))
                return null;
            var contentLength = int.Parse(responseHeaders
                .First(h => h.Name == "content-length")
                .Value);
            //read response body
            var fullBuffer = new byte[contentLength];
            int offset = 0;
            while (true)
            {
                var readResult = await stream.ReadAsync(new ArraySegment<byte>(fullBuffer, offset, contentLength - offset));
                if (readResult.EndOfStream)
                    break;
                offset += readResult.BytesRead;
                if (offset >= contentLength)
                    break;
            }
            var soapReply = Encoding.UTF8.GetString(fullBuffer);
            stream.Cancel();
            return soapReply;
        }

        public async Task Close()
        {
            await ConnectionWrapper.Connection.CloseNow();

            ConnectionWrapper.Socket.Close();
        }
    }
}
