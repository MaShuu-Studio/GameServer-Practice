using System;
using System.Collections.Generic;
using System.Text;

namespace Sample_Server
{
    // 수동으로 하나하나 Task를 만들어서 Queue로 관리하는 방식.
    interface ITask
    {
        void Excute();
    }
    class BroadCastTask : ITask
    {
        GameRoom room;
        ClientSession session;
        string chat;

        BroadCastTask(ClientSession session, string chat)
        {
            this.session = session;
            this.chat = chat;
        }
        public void Excute()
        {
            room.Broadcast(session, chat);
        }
    }
    class TaskQueue
    {
        Queue<ITask> tasks = new Queue<ITask>();
    }
}
