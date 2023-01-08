using NetworkLibrary.Components.Statistics;
using Protobuff.P2P;
using RrlayServerTest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RelayServer
{
    internal class HttpSimpleVisuals
    {
        HttpListener listener = new HttpListener();
        private SecureProtoRelayServer? server;
        string p = "<html><body><body style=\"background-color:black;\"><font color=\"white\"><pre style=\"font-size: large; color: white;\">{0}</pre>";
        string page = "<script>setTimeout(function(){ location.reload();},1000);</script></body></html>";

        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;

        PerformanceCounter total_cpu; //= new PerformanceCounter("Process", "% Processor Time", "_Total");
        PerformanceCounter process_cpu; //= new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);

        DateTime LastUpdate = DateTime.Now.AddSeconds(-2);
        private TcpStatistics TcpGeneralStats;
        private UdpStatistics udpGeneralStats;
        private ConcurrentDictionary<Guid, TcpStatistics> tctSessionStats;
        private ConcurrentDictionary<IPEndPoint, UdpStatistics> udpSessionStats;
        private GeneralStats generalStatJsonObject;

        public HttpSimpleVisuals(SecureProtoRelayServer s)
        {
            ArgumentNullException.ThrowIfNull(s);
            this.server = s;

            
        }

       

        public void BeginService()
        {
            //>netsh http add iplisten ipaddress=0.0.0.0:20012
            //netsh http add urlacl url=http://*:20012/ user=everyone
            listener.Prefixes.Add("http://*:20012/");
            listener.Prefixes.Add("http://*:20012/generalstats/");

            listener.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest req = context.Request;
                    if (req.HttpMethod == "GET")
                    {
                        if (req.RawUrl.Equals("/generalstats", StringComparison.OrdinalIgnoreCase))
                        {
                            PrepareJsonResponse(context);
                        }
                        else if (req.RawUrl.Equals("/text", StringComparison.OrdinalIgnoreCase))
                        {
                            PreparePrettyJsonPageResponse(context);
                        }
                        else
                        {
                            PrepareTextResponse(context);

                        }
                    }

                }
            });
        }

        #region Response
        private void PrepareTextResponse(HttpListenerContext context)
        {
            using HttpListenerResponse resp = context.Response;
            resp.Headers.Set("Content-Type", "text/html");

            Update();

            string data =
                    "Session Count: " + tctSessionStats.Count + "\n" +
                    "Cpu Usage:  " + PerformanceStatistics.GetCpuUsage() + "\n" +
                    "Memory Usage:  " + PerformanceStatistics.GetMemoryUsage() + "\n" +
                    TcpGeneralStats.ToString() + "\n\n" +
                    udpGeneralStats.ToString();

            var page_ = string.Format(p, data) + page;

            byte[] buffer = Encoding.UTF8.GetBytes(page_);
            resp.ContentLength64 = buffer.Length;

            using Stream ros = resp.OutputStream;
            ros.Write(buffer, 0, buffer.Length);
        }
        private void PrepareJsonResponse(HttpListenerContext context)
        {
            using HttpListenerResponse resp2 = context.Response;

            resp2.Headers.Set("Content-Type", "application/json; charset=utf-8");
            resp2.Headers.Set("Access-Control-Allow-Origin", "*");
            byte[] buffer_ = Encoding.UTF8.GetBytes(GetGeneralStatistics());

            resp2.ContentLength64 = buffer_.Length;
            using Stream stream = resp2.OutputStream;
            stream.Write(buffer_, 0, buffer_.Length);
        }
        private void PreparePrettyJsonPageResponse(HttpListenerContext context)
        {
            using HttpListenerResponse resp2 = context.Response;

            resp2.Headers.Set("Content-Type", "text/html");
            resp2.Headers.Set("Access-Control-Allow-Origin", "*");
            byte[] buffer_ = Encoding.UTF8.GetBytes(Resources.StatisticsPage);

            resp2.ContentLength64 = buffer_.Length;
            using Stream stream = resp2.OutputStream;
            stream.Write(buffer_, 0, buffer_.Length);
        }

        #endregion

        public bool Update()
        {
            if ((DateTime.Now - LastUpdate).TotalMilliseconds < 900)
                return false;
            LastUpdate = DateTime.Now;
            server.GetTcpStatistics(out TcpGeneralStats, out tctSessionStats);
            server.GetUdpStatistics(out udpGeneralStats, out udpSessionStats);
            return true;
        }

        public string GetGeneralStatistics()
        {
            if (Update())
            {
                generalStatJsonObject = new GeneralStats()
                {
                    ResourceUsage = new ResourceUsage()
                    {
                        TotalSessions = tctSessionStats.Count.ToString(),
                        CpuUsage = PerformanceStatistics.GetCpuUsage(),
                        RamUsage = PerformanceStatistics.GetMemoryUsage()
                    },
                    TcpGeneralStats = TcpGeneralStats.Stringify(),
                    UdpGeneralStats = udpGeneralStats.Stringify()
                };
            }
            
            return GetJson(generalStatJsonObject);

        }

        public string GetSessionStatistics()
        {
            Update();

            var stats = new Dictionary<Guid, StatisticsJson>();
            foreach (var item in tctSessionStats)
            {
                var stat = new StatisticsJson();
                stat.TcpData = item.Value.Stringify();
                stat.TcpEndpoint = new IpEndpointJsonData(server.GetIPEndPoint(item.Key));
                stats[item.Key] = stat;

            }

            foreach (var item in udpSessionStats)
            {
                if (server.TryGetClientId(item.Key, out var clientId)
                    && stats.ContainsKey(clientId))
                {
                    stats[clientId].UDpData = item.Value.Stringify();
                    stats[clientId].UdpEndpoint = new IpEndpointJsonData(item.Key);
                }
            }
            return GetJson(stats);
        }

        private string GetJson<T>(T data) where T : class
        {
            return JsonSerializer.Serialize<T>(data, new JsonSerializerOptions() { WriteIndented = true });
        }

        public class StatisticsJson
        {
            public IpEndpointJsonData UdpEndpoint { get; set; }
            public IpEndpointJsonData TcpEndpoint { get; set; }

            public TcpStatisticsStringData TcpData { get; set; }

            public UdpStatisticsStringData UDpData { get; set; }

        }
        public class GeneralStats
        {
            public ResourceUsage ResourceUsage { get; set; }
            public TcpStatisticsStringData TcpGeneralStats { get; set; }
            public UdpStatisticsStringData UdpGeneralStats { get; set; }

        }

        public class ResourceUsage
        {
            public string TotalSessions { get; set; }
            public string CpuUsage { get; set; }
            public string RamUsage { get; set; }
        }

        public class IpEndpointJsonData
        {
            public string Ip { get; set; }
            public string Port { get; set; }
            public IpEndpointJsonData(IPEndPoint ep)
            {
                Ip = ep.Address.MapToIPv4().ToString();
                Port = ep.Port.ToString();
            }
        }



    }
}
