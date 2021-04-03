using System;
using System.Collections.Generic;
using System.Text;

namespace Sample_Server_Core
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
            _readPos = _writePos = 0;

        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }
        public ArraySegment<byte> DataSegment // 현재 가용할 수 있는 버퍼의 범위
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }
        public ArraySegment<byte> RecvSegment // 현재 Recv할 수 있는 버퍼의 범위
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0) // 데이터를 다 털어냈을 경우
            {
                _readPos = _writePos = 0;
            }
            else // 데이터가 남아있을 경우
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);

                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize) return false;
            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize) return false;
            _writePos += numOfBytes;
            return true;
        }
    }
}
