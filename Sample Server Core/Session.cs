using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Sample_Server_Core
{
    class Session
    {
        Socket _socket;
        int _disconnect = 0;

        public void Init(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnect, 1) == 1) return;

            _socket.Shutdown(SocketShutdown.Both); // 연결해제 알림
            _socket.Close();
        }

        #region 네트워크 통신
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false) OnRecvCompleted(null, args);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");

                    RegisterRecv(args);
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
