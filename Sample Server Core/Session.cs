using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;
        // sealed: 더이상 오버라이드 불가능.
        // [size(2)][packetid(2)] => 먼저 사이즈가 제대로 도착했는지 확인.
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;
            int packetCount = 0;

            while (true)
            {
                // 적어도 사이즈는 제대로 도착했는지 확인.
                if (buffer.Count < HeaderSize) break;

                // 패킷이 제대로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) break;

                // 패킷이 일단 전부 도착한 것.
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetCount++;

                processLen += dataSize; // 현재 패킷은 도착했으니 처리 진행. 버퍼도 도착한만큼 이동
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }
            if (packetCount > 1) Console.WriteLine($"PacketCount : {packetCount}");
            return processLen;
        }

        // 이 후 패킷의 종류에 따라 Parsing을 진행.
        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnect = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();

        object _lock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock (_lock)
            {
                _pendingList.Clear();
                _sendQueue.Clear();
            }
        }
        public void Init(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(List<ArraySegment<byte>> sendBuffs)
        {
            //_socket.Send(sendBuff);
            // 보낼 때 등록
            //_sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length); // 종료가 되지 않았는데 버퍼가 채워질 수 있음.

            if (sendBuffs.Count == 0) return;

            lock (_lock)
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffs)
                    _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            //_socket.Send(sendBuff);
            // 보낼 때 등록
            //_sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length); // 종료가 되지 않았는데 버퍼가 채워질 수 있음.
            
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnect, 1) == 1) return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both); // 연결해제 알림
            _socket.Close();
            Clear();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            if (_disconnect == 1) return;

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false) OnSendCompleted(null, _sendArgs);
            }
            catch(Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0) // 큐에 내용이 아직 남아있다면
                            RegisterSend(); // 마저 털어냄
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {

                }
            }
            
        }

        void RegisterRecv()
        {
            if (_disconnect == 1) return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.RecvSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false) OnRecvCompleted(null, _recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동. 동시에 문제가 생기면 연결종료
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLen = OnRecv(_recvBuffer.DataSegment);

                    if (processLen < 0)
                    {
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {

            }
        }
        #endregion
    }
}
