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
        static void Main(string[] args)
        {
            // DNS 활용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, PORT_NUMBER);

            // 리스너 생성
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 바인딩
            listenSocket.Bind(endPoint);

            // 리스너 리슨 - 최대 대기자 수 설정
            listenSocket.Listen(10);

            // 무한하게 리슨
            while (true)
            {
                try
                {
                    Console.WriteLine("Listening...");

                    Socket clientSocket = listenSocket.Accept(); // Socket Return. Blocking 함수라서 클라이언트가 올때까지 대기함.

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
        }
    }
}
