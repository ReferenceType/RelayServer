using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer.HttpSimple
{
    internal static class HttpHeaderUtil
    {
        public const string MainPageHeader = @"HTTP/1.1 200 OK
Content-Length: 777
Content-Type: text/html
Server: Microsoft-HTTPAPI/2.0
Access-Control-Allow-Origin: *
Connection: keep-alive
Date: Fri, 27 Jan 2023 18:06:10 GMT

";
        private const string GenericHeader1 = @"HTTP/1.1 200 OK
Content-Length: ";
        private const string GenericHeader2 = @"
Content-Type: text/html
Server: Microsoft-HTTPAPI/2.0
Access-Control-Allow-Origin: *
Connection: keep-alive
Date: Fri, 27 Jan 2023 18:06:10 GMT

";
        static byte[] GenericHeader1Bytes = Encoding.ASCII.GetBytes(GenericHeader1);
        static byte[] GenericHeader2Bytes = Encoding.ASCII.GetBytes(GenericHeader2);
        static byte[] contentLenghtBytes= new byte[4];
        static byte[] headerBytes;
        static HttpHeaderUtil()
        {
            headerBytes = GenericHeader1Bytes.Concat(contentLenghtBytes).Concat(GenericHeader2Bytes).ToArray();
        }
        public static byte[] GetASCIIHeader(int contentLenght)
        {
            var contentLenghtBytes=Encoding.ASCII.GetBytes(contentLenght.ToString());
            Buffer.BlockCopy(contentLenghtBytes, 0, headerBytes, 33, contentLenghtBytes.Length);
            return GenericHeader1Bytes.Concat(contentLenghtBytes).Concat(GenericHeader2Bytes).ToArray();
        }
    }
}
