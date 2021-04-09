using System;
using System.Collections.Generic;
using System.Text;

namespace Sample_Server
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{packet.playerId}: {chat}";
            ArraySegment<byte> segment = packet.Write();

            // 프로젝트가 커짐에 따라 패킷이 몰리면서 문제가 생김.
            // 이 후 Queue 등을 활용해 대기시켜놓는 방식 활용.
            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                    s.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Remove(session);
            }
        }
    }
}
