using NetworkLibrary.Components;
using ProtoBuf;
using Protobuff;
using Protobuff.P2P;
using System.Security.Cryptography.X509Certificates;

namespace RrlayServerTest
{
    //KFtyEu7h6E
    internal class Program
    {
        [ProtoContract]
        public class PeerList
        {
            [ProtoMember(1)]
            public Guid asd;


        }
        static void Main(string[] args)
        {
           
            var scert = new X509Certificate2("server.pfx", "greenpass");
            var server = new SecureProtoRelayServer(20011, 1000, scert);
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    server.GetTcpStatistics(out SessionStats generalStats, out var a);
                    server.GetUdpStatistics(out UdpStatistics generalStatsU, out _);
                    (int left,int right )=Console.GetCursorPosition();
                    Console.Clear();
                    Console.WriteLine("Session Count: "+a.Count);
                    Console.WriteLine(generalStats.ToString());
                    Console.WriteLine(generalStatsU.ToString());
                    if(left != 0 && right != 0)
                        Console.SetCursorPosition(left, right);
                }

            });
            while (Console.ReadLine() != "e")
            {

            }
        }
    }
}