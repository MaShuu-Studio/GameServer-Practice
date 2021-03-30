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
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff); // 받은 버퍼의 크기
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes); // 시작지점부터 받은 크기까지
                Console.WriteLine($"[From Client] {recvData}");

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome!");
                clientSocket.Send(sendBuff);

                clientSocket.Shutdown(SocketShutdown.Both); // 연결해제 알림
                clientSocket.Close();
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
