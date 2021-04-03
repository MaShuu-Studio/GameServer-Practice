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
        class GameSession : Session
        {
            public override void OnConnected(EndPoint endPoint)
            {
                Console.WriteLine($"[System] OnConnected : {endPoint}");

                try
                {
                    for (int i = 0; i < 5; i++)
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World");
                        Send(sendBuff);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Error]" + e.ToString());
                }
            }

            public override void OnDisconnected(EndPoint endPoint)
            {
                Console.WriteLine($"[System] OnDisconnected : {endPoint}");
            }

            public override int OnRecv(ArraySegment<byte> buffer)
            {
                string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
                Console.WriteLine($"[From Server] {recvData}");

                return buffer.Count;
            }

            public override void OnSend(int numOfBytes)
            {
                Console.WriteLine($"[System] Transferred bytes: {numOfBytes}");
            }
        }
        const int PORT_NUMBER = 7777;
        static void Main(string[] args)
        {
            // DNS 활용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, PORT_NUMBER);
            
            Connector connector = new Connector();


            while (true)
            {
                try
                {
                    connector.Connect(endPoint, () => { return new GameSession(); });
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Error]" + e.ToString());
                }

                Thread.Sleep(1000);
            }
        }
    }
}
