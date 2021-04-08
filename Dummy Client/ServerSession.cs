using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Sample_Server_Core;

namespace Dummy_Client
{
    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[System] OnConnected : {endPoint}");

            try
            {
                PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "ABCD"};
				var skill = new PlayerInfoReq.Skill() { id = 0, level = 1, duration = 1.1f };
				skill.attrubutes.Add(new PlayerInfoReq.Skill.Attrubute() { attName = 53 });
                packet.skills.Add(skill);
                packet.skills.Add(new PlayerInfoReq.Skill() { id = 2, level = 5, duration = 3.0f });
                packet.skills.Add(new PlayerInfoReq.Skill() { id = 3, level = 7, duration = 7.5f });
                packet.skills.Add(new PlayerInfoReq.Skill() { id = 4, level = 2, duration = 1.0f });
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
