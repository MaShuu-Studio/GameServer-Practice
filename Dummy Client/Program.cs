using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sample_Server_Core;

namespace Dummy_Client
{
    class Program
    {
        const int PORT_NUMBER = 7777;
        static void Main(string[] args)
        {
            // DNS 활용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, PORT_NUMBER);
            
            Connector connector = new Connector();

            connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); }, 100);

            while (true)
            {
                try
                {
                    SessionManager.Instance.SendForEach();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Error]" + e.ToString());
                }

                Thread.Sleep(250);
            }
        }
    }
}
