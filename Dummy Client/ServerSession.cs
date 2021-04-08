using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Sample_Server_Core;

namespace Dummy_Client
{
	class PlayerInfoReq
	{
		public byte testByte;
		public long playerId;
		public string name;
		public class Skill
		{
			public class Attrubute
			{
				public int attName;

				public void Read(ReadOnlySpan<byte> s, ref ushort count)
				{
					this.attName = BitConverter.ToInt32(s.Slice(count, s.Length - count));
					count += sizeof(int);
				}
				public bool Write(Span<byte> s, ref ushort count)
				{
					bool success = true;
					success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attName);
					count += sizeof(int);

					return success;
				}
			}
			public List<Attrubute> attrubutes = new List<Attrubute>();
			public int id;
			public ushort level;
			public float duration;

			public void Read(ReadOnlySpan<byte> s, ref ushort count)
			{
				attrubutes.Clear();
				ushort attrubuteLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
				count += sizeof(ushort);
				for (int i = 0; i < attrubuteLen; i++)
				{
					Attrubute attrubute = new Attrubute();
					attrubute.Read(s, ref count);
					this.attrubutes.Add(attrubute);
				}
				this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
				count += sizeof(int);
				this.level = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
				count += sizeof(ushort);
				this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
				count += sizeof(float);
			}
			public bool Write(Span<byte> s, ref ushort count)
			{
				bool success = true;
				success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)attrubutes.Count);
				count += sizeof(ushort);
				foreach (Attrubute attrubute in attrubutes)
					success &= attrubute.Write(s, ref count);
				success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
				count += sizeof(int);

				success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
				count += sizeof(ushort);

				success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
				count += sizeof(float);

				return success;
			}
		}
		public List<Skill> skills = new List<Skill>();

		public void Read(ArraySegment<byte> segment)
		{
			ushort count = 0;

			ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

			count += sizeof(ushort);
			count += sizeof(ushort);

			this.testByte = (byte)segment.Array[segment.Offset + count];
			count += sizeof(byte);
			this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
			count += sizeof(long);
			ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
			count += nameLen;
			skills.Clear();
			ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			for (int i = 0; i < skillLen; i++)
			{
				Skill skill = new Skill();
				skill.Read(s, ref count);
				this.skills.Add(skill);
			}
		}

		public ArraySegment<byte> Write()
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
			bool success = true;
			ushort count = 0;

			Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

			count += sizeof(ushort);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
			count += sizeof(ushort);

			segment.Array[segment.Offset + count] = (byte)this.testByte;
			count += sizeof(byte);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
			count += sizeof(long);

			ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
			count += sizeof(ushort);
			count += nameLen;

			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
			count += sizeof(ushort);
			foreach (Skill skill in skills)
				success &= skill.Write(s, ref count);

			success &= BitConverter.TryWriteBytes(s, count);

			if (!success) return null;
			return SendBufferHelper.Close(count);
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
