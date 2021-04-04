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
        public class Packet
        {
            public ushort size; // 사이즈를 통해 모든 패킷이 왔는지 확인함.
            public ushort packetId; //어떠한 패킷인지 확인함.
        }
        class GameSession : Session
        {
            public override void OnConnected(EndPoint endPoint)
            {
                Console.WriteLine($"[System] OnConnected : {endPoint}");

                try
                {
                    Packet packet = new Packet() { size = 4, packetId = 10 };
                    for (int i = 0; i < 5; i++)
                    {
                        ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

                        byte[] buffer = BitConverter.GetBytes(packet.size);
                        byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
                        Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
                        Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);

                        ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

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

                Thread.Sleep(5000);
            }
        }
    }
}
