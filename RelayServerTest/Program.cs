using NetworkLibrary;
using NetworkLibrary.Components;
using NetworkLibrary.Components.Statistics;
using NetworkLibrary.Utils;
using ProtoBuf;
using Protobuff;
using Protobuff.P2P;
using RelayServer;
using RelayServer.HttpSimple;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace RrlayServerTest
{
    //KFtyEu7h6E
    public class Config
    {
        public int PortRelay { get; set; }
        public int PortHttp { get; set; }
        public bool UpdateProxy { get; set; }
        public string ProxyUri { get; set; }
    }
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
        static Config config;
        private static string uri;
        static ManualResetEvent m = new ManualResetEvent(false);

        public static string Ip;
        public static string Port;
        static void Main(string[] args)
        {
            MiniLogger.AllLog += (string str)=>Console.WriteLine(str);
            Configure();
            uri = config.ProxyUri;
            var port = config.PortRelay;
            Port = port.ToString();
            
            var scert = new X509Certificate2("server.pfx", "greenpass");
            var server = new SecureProtoRelayServer(port, scert);

            var hserv = new SimpleHttpServer(server,config.PortHttp);
            //hserv.BeginService();

            if (config.UpdateProxy)
            {
               PeriodicallyUpdateHttpProxy();
            }
            m.WaitOne();
        }

        private static void Configure()
        {
            string workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string txt = File.ReadAllText(workingDir + "/Config.json");
            config = JsonSerializer.Deserialize<Config>(txt,new JsonSerializerOptions() { AllowTrailingCommas = true});

            /* original limits. This represents the allowed size before trimming.
            { 256,10000 },
            { 512,10000 },
            { 1024,10000 },
            { 2048,5000 },
            { 4096,1000 },
            { 8192,1000 },
            { 16384,500 },
            { 32768,300 },
            { 65536,300 },
            { 131072,200 },
            { 262144,50 },
            { 524288,10 },
            { 1048576,4 },
            { 2097152,2 },
            { 4194304,1 },
            { 8388608,1 },
            { 16777216,1 },
            { 33554432,0 },
            { 67108864,0 },
            { 134217728,0 },
            { 268435456,0 },
            { 536870912,0 },
            { 1073741824,0 }
             */
            BufferPool.ForceGCOnCleanup = true;
            BufferPool.SetBucketLimitBySize(257, 1000);
            BufferPool.SetBucketLimitBySize(512, 1000);
            BufferPool.SetBucketLimitBySize(1024, 500);
            BufferPool.SetBucketLimitBySize(2048, 250);
            BufferPool.SetBucketLimitBySize(4096, 250);
            BufferPool.SetBucketLimitBySize(8192, 250);
            BufferPool.SetBucketLimitBySize(16384, 100);
            BufferPool.SetBucketLimitBySize(32768, 100);
            BufferPool.SetBucketLimitBySize(65536, 50);
            BufferPool.SetBucketLimitBySize(131072, 10);
            BufferPool.SetBucketLimitBySize(262144, 10);
            BufferPool.SetBucketLimitBySize(524288, 10);
            BufferPool.SetBucketLimitBySize(1048576, 5);
            BufferPool.SetBucketLimitBySize(2097152, 0);
            BufferPool.SetBucketLimitBySize(4194304, 0);
            BufferPool.SetBucketLimitBySize(8388608, 0);
            BufferPool.SetBucketLimitBySize(16777216, 0);
            
    }

        private static void PeriodicallyUpdateHttpProxy()
        {
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

                        Ip = IPinfo.ip;

                        var updateMsg = JsonSerializer.Serialize(adressUpdate);
                        await cl.PostAsync(uri, new StringContent(updateMsg, System.Text.Encoding.UTF8, "application/json"));
                        await Task.Delay(60000);
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(300000);
                    }
                }



            });
        }
    }
}