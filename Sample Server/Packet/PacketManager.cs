using Sample_Server_Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample_Server
{
    class PacketManager
    {
        static PacketManager instance;
        public static PacketManager Instance
        {
            get
            {
                if (instance == null) instance = new PacketManager();
                return instance;
            }
        }

        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
        Dictionary<ushort, Action<PacketSession, IPacket>> handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
        public void Register()
        {
            onRecv.Add((ushort)PacketID.PlayerInfoReq, MakePacket<PlayerInfoReq>);
            handler.Add((ushort)PacketID.PlayerInfoReq, PacketHandler.PlayerInfoReqHandler);
        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            int count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            Action<PacketSession, ArraySegment<byte>> action = null;
            if (onRecv.TryGetValue(id, out action))
                action.Invoke(session, buffer);
        }

        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
        {
            T packet = new T();
            packet.Read(buffer);

            Action<PacketSession, IPacket> action = null;
            if (handler.TryGetValue(packet.Protocol, out action))
                action.Invoke(session, packet);
        }
    }
}
