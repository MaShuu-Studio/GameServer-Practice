using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Sample_Server_Core
{
    class Program
    {
        const int PORT_NUMBER = 7777;

        static Listener _listener = new Listener();
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Init(clientSocket);

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome!");
                session.Send(sendBuff);

                Thread.Sleep(1000);

                session.Disconnect();
                session.Disconnect();


            }
            catch (Exception e)
            {
                Console.WriteLine("[Error]" + e.ToString());
            }
        }

        static void Main(string[] args)
        {
            // DNS 활용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, PORT_NUMBER);

            _listener.init(endPoint, OnAcceptHandler);
            Console.WriteLine("Listening...");

            while (true)
            {

            }
        }
    }
}
