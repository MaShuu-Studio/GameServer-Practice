using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample_Server_Core
{
    class Program
    {
        static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 1000000; i++)
            {
                try
                {
                    // Critical Section에 진입 시도
                    Monitor.Enter(_obj);
                    number++;
                }
                finally
                {
                    // Critical Section에서 나옴
                    Monitor.Exit(_obj);
                }

                // 일반적으로 직접적으로 관리하기 힘들기 때문에 try-catch finally를 활용 가능
                // 다만 이 경우도 불편하기 때문에 lock 구문 활용

                lock (_obj)
                {
                    number++;
                }
            }    
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000; i++)
            {
                Monitor.Enter(_obj);
                number--;
                Monitor.Exit(_obj);
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}
