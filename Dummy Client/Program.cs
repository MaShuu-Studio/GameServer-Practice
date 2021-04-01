using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
            
            while (true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Connect(endPoint); // 상대주소에 연결 요청
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    for (int i = 0; i < 5; i++)
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World");
                        socket.Send(sendBuff);
                    }

                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server] {recvData}");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
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
