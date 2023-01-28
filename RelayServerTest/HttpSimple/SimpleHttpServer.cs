using NetworkLibrary.Components.Statistics;
using NetworkLibrary.TCP.Base;
using NetworkLibrary.Utils;
using Protobuff.P2P;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static RelayServer.HttpSimple.Alternative.HttpNativeServer;

namespace RelayServer.HttpSimple
{
    internal class SimpleHttpServer
    {
        private byte[] mainPageBytes;
        // this is the one periodically requested by main page.
        const string textPagePart1 = "<html><body><body style=\"background-color:black;\"><font color=\"white\"><pre style=\"font-size: large; color: white;\">";
        const string textPagePart2 = "</pre><script>setTimeout(function(){ location.reload();},1000);</script></body></html>";

        private DateTime LastUpdate = DateTime.Now.AddSeconds(-2);
        private TcpStatistics TcpGeneralStats;
        private UdpStatistics udpGeneralStats;
        private ConcurrentDictionary<Guid, TcpStatistics> tcpSessionStats;
        private ConcurrentDictionary<IPEndPoint, UdpStatistics> udpSessionStats;

        private SecureProtoRelayServer server;
        private AsyncTcpServer httpMiniServer;
        private SharerdMemoryStreamPool streamPool =  new SharerdMemoryStreamPool();
        public SimpleHttpServer(SecureProtoRelayServer s, int porthttp) 
        {
            server = s;

            var mainPageHeaderBytes = Encoding.ASCII.GetBytes(HttpHeaderUtil.MainPageHeader);
            var mainPageBodybytes = Encoding.UTF8.GetBytes(PageResources.TextVisualizePage);
            mainPageBytes = mainPageHeaderBytes.Concat(mainPageBodybytes).ToArray();
            
            httpMiniServer = new AsyncTcpServer(porthttp);
            httpMiniServer.GatherConfig = ScatterGatherConfig.UseQueue;
            httpMiniServer.MaxIndexedMemoryPerClient = 128000;
            httpMiniServer.OnBytesReceived += ServerBytesReceived;
            httpMiniServer.StartServer();
        }
        private void ServerBytesReceived(in Guid guid, byte[] bytes, int offset, int count)
        {
            try
            {
                var request = UTF8Encoding.ASCII.GetString(bytes, offset, 10);
                var tags = request.Split(' ');
                if (tags[1] == "/")
                {
                    httpMiniServer.SendBytesToClient(guid, mainPageBytes);
                }
                else if (tags[1] == "/text")
                {
                    UpdateStatistics();
                    string data =
                           "Session Count: " + tcpSessionStats.Count + "\n" +
                           "Cpu Usage:  " + PerformanceStatistics.GetCpuUsage() + "\n" +
                           "Memory Usage:  " + PerformanceStatistics.GetMemoryUsage() + "\n" +
                           TcpGeneralStats.ToString() + "\n\n" +
                           udpGeneralStats.ToString();

                    string page = textPagePart1 + data + textPagePart2;
                    byte[] body = Encoding.UTF8.GetBytes(page);
                    byte[] header = HttpHeaderUtil.GetASCIIHeader(body.Length);
                    
                    httpMiniServer.SendBytesToClient(in guid, header);
                    httpMiniServer.SendBytesToClient(in guid, body);
                    
                }
            } catch { }
           
        }

        private void GetTextResponse(Stream stream)
        {
            UpdateStatistics();
            string data =
                   "Session Count: " + tcpSessionStats.Count + "\n" +
                   "Cpu Usage:  " + PerformanceStatistics.GetCpuUsage() + "\n" +
                   "Memory Usage:  " + PerformanceStatistics.GetMemoryUsage() + "\n" +
                   TcpGeneralStats.ToString() + "\n\n" +
                   udpGeneralStats.ToString();

            string page = textPagePart1 + data + textPagePart2;
            byte[] body = Encoding.UTF8.GetBytes(page);
            byte[] header = HttpHeaderUtil.GetASCIIHeader(body.Length);

            stream.Position = 0;
            stream.Write(header, 0, header.Length);
            stream.Write(body, 0, body.Length);

        }

        public bool UpdateStatistics()
        {
            if ((DateTime.Now - LastUpdate).TotalMilliseconds < 900)
                return false;
            LastUpdate = DateTime.Now;
            try
            {
                server.GetTcpStatistics(out var tcpGeneralStats, out var tcpSessionStats);
                server.GetUdpStatistics(out var udpGeneralStats, out var udpSessionStats);

                TcpGeneralStats = tcpGeneralStats;
                this.tcpSessionStats = tcpSessionStats;
                this.udpGeneralStats = udpGeneralStats;
                this.udpSessionStats = udpSessionStats;
            }
            catch (Exception ex) { return false; }

            return true;
        }
    }
}
