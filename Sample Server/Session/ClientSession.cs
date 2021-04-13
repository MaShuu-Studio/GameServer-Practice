using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;
using ServerCore;
using System.Collections.Generic;

namespace SampleServer
{       
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[System] OnConnected : {endPoint}");

            Program.Room.Push(() => Program.Room.Enter(this));
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[System] OnDisconnected : {endPoint}");

            if (Room != null)
            {
                // 실행 순서가 Queue에 쌓임에 따라 Room을 직접 찾아가는 행위는 문제가 될 수 있음.
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;
            }
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"[System] Transferred bytes: {numOfBytes}");
        }

    }
}
