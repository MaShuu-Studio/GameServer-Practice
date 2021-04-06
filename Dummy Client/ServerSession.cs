using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sample_Server_Core;

namespace Dummy_Client
{
    public abstract class Packet
    {
        public ushort size; // 사이즈를 통해 모든 패킷이 왔는지 확인함.
        public ushort packetId; //어떠한 패킷인지 확인함.

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            packetId = (ushort)PacketID.PlayerInfoReq;
        }
        public override void Read(ArraySegment<byte> buffer)
        {
            int count = 0;

            //ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            //ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            // 전송된 패킷에 문제가 있더라도 제대로ㅓ 잘 체크할 수 있도록 해줘야함.
            playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(buffer.Array, buffer.Offset + count, buffer.Count - count));
            count += 8;

        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            bool success = true;
            ushort count = 0;

            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), this.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), this.playerId);
            count += 8;

            this.size = count;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), size);

            if (!success) return null;

            return SendBufferHelper.Close(size);
        }
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
                PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001 };
                //for (int i = 0; i < 5; i++)
                {
                    ArraySegment<byte> sendBuff = packet.Write();
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
