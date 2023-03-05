using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RelayServer
{
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

    internal class ProxyPeriodicUpdate
    {
        private static string uri;
        public static string Ip;
        public static string Port;
        public static void PeriodicallyUpdateHttpProxy()
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
