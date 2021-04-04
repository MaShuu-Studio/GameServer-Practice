using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;

using Sample_Server_Core;

namespace Sample_Server
{
    class GameSession : PacketSession
    {
        public class Packet
        {
            public ushort size; // 사이즈를 통해 모든 패킷이 왔는지 확인함.
            public ushort packetId; //어떠한 패킷인지 확인함.
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[System] OnConnected : {endPoint}");

            try
            {
                //Packet packet = new Packet() { size = 4, packetId = 10 };
                //
                //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                //
                //byte[] buffer = BitConverter.GetBytes(packet.size);
                //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
                //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
                //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
                //
                //ArraySegment<byte> sendBuff = SendBufferHelper.Close(pakcet.size);

                //Send(sendBuff);

                Thread.Sleep(5000);

                Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error]" + e.ToString());
            }
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"[From Client] RecvPacketId: {id}, Size: {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[System] OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[System] Transferred bytes: {numOfBytes}");
        }

    }
    class Program
    {
        const int PORT_NUMBER = 7777;

        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS 활용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, PORT_NUMBER);

            _listener.init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening...");

            while (true)
            {

            }
        }
    }
}
