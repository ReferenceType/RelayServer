using NetworkLibrary.Components;
using NetworkLibrary.Components.Statistics;
using ProtoBuf;
using Protobuff;
using Protobuff.P2P;
using System;
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
        
    {
        private static readonly string uri = @"http://localhost:8001/";
        static ManualResetEvent m = new ManualResetEvent(false);
       
        static void Main(string[] args)
        {
           
            var scert = new X509Certificate2("server.pfx", "greenpass");
            var server = new SecureProtoRelayServer(20011, 1000, scert);
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
                        var update = new UpdateAdress()
                        {
                            Ip = IPinfo.ip,
                            Id = "IPass"
                        };

                        var updateMsg = JsonSerializer.Serialize(update);
                        await cl.PostAsync(uri, new StringContent(updateMsg));
                        await Task.Delay(60000);
                    }
                    catch (Exception ex) { }
                   

                }



            });
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    server.GetTcpStatistics(out SessionStats generalStats, out var a);
                    server.GetUdpStatistics(out UdpStatistics generalStatsU, out _);
                    (int left, int right) = Console.GetCursorPosition();
                    Console.Clear();
                    Console.WriteLine("Session Count: " + a.Count);
                    Console.WriteLine(generalStats.ToString());
                    Console.WriteLine(generalStatsU.ToString());
                    if (left != 0 && right != 0)
                        Console.SetCursorPosition(left, right);
                }

            });

            m.WaitOne();
            while (Console.ReadLine() != "e")
            {

            }
        }
    }
}