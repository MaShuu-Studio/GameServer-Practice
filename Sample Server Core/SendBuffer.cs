using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sample_Server_Core
{
    public class SendBufferHelper
    {
        // 쓰레드 끼리의 경합을 없앰.
        // Buffer에 쓰기를 시행하는 부분은 ThreadLocal을 활용하여 방지를 하였고
        // 이 후는 읽기만 하기 때문에 멀티쓰레드 문제 X
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null || CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }
    public class SendBuffer
    {
        // 보내는 데이터에 따라 가변적인 성격을 가지고 있을 수 있고
        // 데이터의 활용을 위해 커다란 버퍼를 만들어두고 조금씩 잘라 보내는 식으로 진행.
        // 다만 데이터 전송의 특성상 버퍼를 참조할 수 있기 때문에 재활용은 불가능.
        byte[] _buffer;
        int _usedSize = 0;

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize) return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;

            return segment;
        }
    }
}
