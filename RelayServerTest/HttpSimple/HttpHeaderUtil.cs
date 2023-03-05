using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer.HttpSimple
{
    internal static class HttpHeaderUtil
    {
        public const string MainPageHeader = @"HTTP/1.1 200 OK
Content-Type: text/html
Server: Microsoft-HTTPAPI/2.0
Access-Control-Allow-Origin: *
Connection: keep-alive
Date: Fri, 27 Jan 2023 18:06:10 GMT
Content-Length: 777


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
        static byte[] contentLenghtBytes= new byte[3];
        static byte[] headerBytes;
        static HttpHeaderUtil()
        {
            headerBytes = GenericHeader1Bytes.Concat(contentLenghtBytes).Concat(GenericHeader2Bytes).ToArray();
        }

        public static byte[] GetASCIIHeader(int contentLenght)
        {
            var contentLenghtBytes = Encoding.ASCII.GetBytes(contentLenght.ToString());
            return GenericHeader1Bytes.Concat(contentLenghtBytes).Concat(GenericHeader2Bytes).ToArray();
        }

        public static void GetASCIIHeader(Stream stream, int contentLenght)
        {
            stream.Write(GenericHeader1Bytes, 0, GenericHeader1Bytes.Length);

            var contentLenghtBytes = Encoding.ASCII.GetBytes(contentLenght.ToString());
            stream.Write(contentLenghtBytes, 0, contentLenghtBytes.Length);

            stream.Write(GenericHeader2Bytes, 0, GenericHeader2Bytes.Length);


        }
    }
}
