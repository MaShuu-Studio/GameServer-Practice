using System;
using System.Collections.Generic;
using System.Text;

namespace Packet_Generator
{
    class PacketFormat
    {
        // {0} 패킷 이름
        // {1} 멤버 변수
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static string packetFormat =
@"class {0}
{{
    {1}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);
        
        {2}
    }}

    public ArraySegment<byte> Write()
    {{ 
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);
        
        {3}

        success &= BitConverter.TryWriteBytes(s, count);

        if (!success) return null;
        return SendBufferHelper.Close(count);
    }}
}}";

        // {0} 변수 타입
        // {1} 변수 이름
        public static string memberFormat =
@" public {0} {1};";
        // {0} 변수 이름
        // {1} 변수 컨버팅 타입
        // {2} 변수 타입
        public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

        // {0} 변수 이름
        public static string stringReadFormat =
@"ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";

        // {0} 변수 이름
        // {1} 변수 타입
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});
";

        // {0} 변수 이름
        // {1} 변수 타입
        public static string stringWriteFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;
";
    }
}
