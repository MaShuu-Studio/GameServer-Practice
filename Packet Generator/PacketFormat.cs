using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
    class PacketFormat
    {
        // {0} Register Code
        public static string managerFormat =
@"
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{{
    static PacketManager instance = new PacketManager();
    public static PacketManager Instance {{ get {{ return instance; }} }}

    PacketManager()
    {{
        Register();
    }}

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
    public void Register()
    {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {{
        int count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (makeFunc.TryGetValue(id, out func))
        {{
            IPacket packet = func.Invoke(session, buffer);

            // 유니티에서는 게임쓰레드가 아닌곳에서 유니티코드를 불러올 수 없게 되어있음.
            // 따라서 패킷 생성파트와 핸들파트를 분리하고 작업을 큐에 보관 후 게임쓰레드에서 실행
            // 이를 옵션으로 받아서 패킷큐에 푸시하는 식으로 진행.

            if (onRecvCallback != null) onRecvCallback.Invoke(session, packet);
            else HandlePacket(session, packet);
        }}
    }}

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T packet = new T();
        packet.Read(buffer);

        return packet;
    }}

    public void HandlePacket(PacketSession session, IPacket packet)
    {{
        Action<PacketSession, IPacket> action = null;
        if (handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }}
}}";

        // {0} 패킷 이름
        public static string managerRegisterFormat =
@"        makeFunc.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);";


        // {0} 패킷 번호
        // {1} 패킷 목록
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ServerCore;

public enum PacketID
{{
    {0}
}}

public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

{1}";
        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        
        // {0} 패킷 이름
        // {1} 멤버 변수
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static string packetFormat =
@"class {0} : IPacket
{{
    {1}
    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        count += sizeof(ushort);
        count += sizeof(ushort);
        
        {2}
    }}

    public ArraySegment<byte> Write()
    {{ 
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.{0}), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        
        {3}

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }}
}}
";

        // {0} 변수 타입
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"public class {0}
{{
    {2}

    public void Read(ArraySegment<byte> segment, ref ushort count)
    {{
        {3}
    }}
    public bool Write(ArraySegment<byte> segment, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}
}}
public List<{0}> {1}s = new List<{0}>();";

        // {0} 변수 이름
        // {1} 변수 컨버팅 타입
        // {2} 변수 타입
        public static string readFormat =
@"this.{0} = BitConverter.{1}(segment.Array, segment.Offset + count);
count += sizeof({2});";

        // {0} 변수 이름
        // {1} 변수 타입
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});";

        // {0} 변수 이름
        public static string stringReadFormat =
@"ushort {0}Len = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, {0}Len);
count += {0}Len;";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string listReadFormat =
@"{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    this.{1}s.Add({1});
}}";
        // {0} 변수 이름
        // {1} 변수 타입
        public static string writeFormat =
@"Array.Copy(BitConverter.GetBytes(this.{0}), 0, segment.Array, segment.Offset + count, sizeof({1}));
count += sizeof({1});
";

        // {0} 변수 이름
        // {1} 변수 타입
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        // {0} 변수 이름
        // {1} 변수 타입
        public static string stringWriteFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
Array.Copy(BitConverter.GetBytes({0}Len), 0, segment.Array, segment.Offset + count, sizeof(ushort));
count += sizeof(ushort);
count += {0}Len;
";
        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string listWriteFormat =
@"Array.Copy(BitConverter.GetBytes((ushort){1}s.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
count += sizeof(ushort);
foreach({0} {1} in {1}s)
    success &= {1}.Write(s, ref count);";
    }
}
