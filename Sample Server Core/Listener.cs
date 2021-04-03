using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sample_Server_Core
{
    class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            // 리스너 생성
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // 바인딩
            _listenSocket.Bind(endPoint);

            // 리스너 리슨 - 최대 대기자 수 설정
            _listenSocket.Listen(10);

            // 초기화 하면서 등록. 요청이 들어오면 Callback 방식으로 OnAccpetCompleted 작동.
            // RegisterAccept를 실행했을 때 바로 요청이 들어왔다면 pending이 false가 되어 작동
            // 그게 아니라면 pending == false 부분은 넘어가겠지만 후에 Callback으로 Completed에 넣어둔 OnAcceptCompleted 작동\
            for (int i = 0; i < 5; i++) // 다수의 리스너 사용
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null; // 기존 Socket을 비워줌

            bool pending = _listenSocket.AcceptAsync(args); // 비동기함수.
            if (pending == false) OnAcceptCompleted(null, args); // 즉각적으로 요청이 들어와 바로 처리
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success) // 성공
            {
                // Listener에서 connect가 들어가기 때문에 Seesion 의 위치 이동.

                // 함수의 통일 및 리스너별 컨텐츠 세션 분리를 위해 Func<> 사용.
                // 이후 return 된 Session Factory에 넣어 통일된 함수 사용. 실질적 작동은 Override 된 함수를 활용
                Session session = _sessionFactory.Invoke();
                session.Init(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args); // Accept가 끝난 이후 다음을 위해 재 등록.
        }
    }
}
