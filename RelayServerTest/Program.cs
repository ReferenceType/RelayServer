using NetworkLibrary.Components;
using NetworkLibrary.Components.Statistics;
using ProtoBuf;
using Protobuff;
using Protobuff.P2P;
using RelayServer;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace RrlayServerTest
{
    //KFtyEu7h6E
    public class Adress
    {
        public string ip { get; set; }
        public string country { get; set; }
        public string cc { get; set; }
    }

    public class UpdateAdress
    {
        public string Ip { get; set; }

        public string Id { get; set; }
    }

    internal class Program
    //http://localhost:61953/aaa
    {
        private static readonly string uri = @"http://localhost:8001/ip";
        static ManualResetEvent m = new ManualResetEvent(false);

        public static string Ip;
        public static string Port;
        static void Main(string[] args)
        {
            var port = 20011;
            Port = port.ToString();
            // I have a Http proxy server that i intend to put on cloud
            // that gives the ip if this relay server whis is behind my router and ip isnt static
            var scert = new X509Certificate2("server.pfx", "greenpass");
            var server = new SecureProtoRelayServer(port, scert);

            var hserv = new HttpSimpleVisuals(server);
            hserv.BeginService();


            Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        HttpClient cl = new HttpClient();

                        var uri_ = @"https://api.myip.com";
                        var result = await cl.GetAsync(uri_);
                        var str = await result.Content.ReadAsStringAsync();

                        var IPinfo = JsonSerializer.Deserialize<Adress>(str);
                        var adressUpdate = new UpdateAdress()
                        {
                            Ip = IPinfo.ip,
                            Id = "IPass"
                        };

                        Ip=IPinfo.ip;
                      
                        var updateMsg = JsonSerializer.Serialize(adressUpdate);
                        await cl.PostAsync(uri, new StringContent(updateMsg,System.Text.Encoding.UTF8,"application/json"));
                        await Task.Delay(60000);
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(30000);
                    }


                }



            });
            #region Test
            Task.Run(async () =>
            {
                return;
                while (true)
                {
                    await Task.Delay(1010);
                    Console.Clear();
                    //Console.WriteLine(PerformanceStatistics.GetCpuUsage());
                    //Console.WriteLine(PerformanceStatistics.GetMemoryUsage());

                    string stats = hserv.GetGeneralStatistics();
                    Console.WriteLine(stats);
                    

                    //Console.WriteLine(hserv.GetSessionStatistics());
                    //server.GetTcpStatistics(out SessionStats generalStats, out var a);
                    //server.GetUdpStatistics(out UdpStatistics generalStatsU, out _);
                    //(int left, int right) = Console.GetCursorPosition();
                    //Console.Clear();
                    //Console.WriteLine("Session Count: " + a.Count);
                    //Console.WriteLine(generalStats.ToString());
                    //Console.WriteLine(generalStatsU.ToString());
                    //if (left != 0 && right != 0)
                    //    Console.SetCursorPosition(left, right);
                }

            });

            //Thread.Sleep(10000);
            //Parallel.For(0, 8, (i) =>
            //{
            //    while (true)
            //    {
            //        int k = 0;
            //        k++;
            //        k--;
            //    }
            //}
            //);
            #endregion
            m.WaitOne();

        }
       
    }
}