using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sample_Server_Core;

namespace Dummy_Client
{
    public class Packet
    {
        public ushort size; // 사이즈를 통해 모든 패킷이 왔는지 확인함.
        public ushort packetId; //어떠한 패킷인지 확인함.
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
    }

    class PlayerInfoOk : Packet
    {
        public int hp;
        public int atk;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[System] OnConnected : {endPoint}");

            try
            {
                PlayerInfoReq packet = new PlayerInfoReq() { packetId = (ushort)PacketID.PlayerInfoReq, playerId = 1001 };
                //for (int i = 0; i < 5; i++)
                {
                    ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                    bool success = true;
                    ushort count = 0;

                    count += 2;
                    success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), packet.packetId);
                    count += 2;
                    success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), packet.playerId);
                    count += 8;

                    packet.size = count;
                    success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), packet.size);
                    
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
}
