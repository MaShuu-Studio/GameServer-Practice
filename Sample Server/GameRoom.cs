using System;
using System.Collections.Generic;
using System.Text;
using Sample_Server_Core;

namespace Sample_Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{packet.playerId}: {chat}";
            ArraySegment<byte> segment = packet.Write();

            // 프로젝트가 커짐에 따라 패킷이 몰리면서 문제가 생김.
            // 이 후 Queue 등을 활용해 대기시켜놓는 방식 활용.
            // Queue를 Lock을 걸어 사용함에 따라 각 action들은 lock을 할 필요가 없어짐.
            foreach (ClientSession s in _sessions)
                s.Send(segment);
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}
