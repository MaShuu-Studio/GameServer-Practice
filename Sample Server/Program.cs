using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Sample_Server_Core;

namespace Sample_Server
{
    class Program
    {
        const int PORT_NUMBER = 7777;

        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            PacketManager.Instance.Register(); // 싱글쓰레드에서 첫 발동만 해야함

            // DNS 활용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, PORT_NUMBER);

            _listener.init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("Listening...");

            while (true)
            {

            }
        }
    }
}
