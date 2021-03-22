using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample_Server_Core
{
    class Lock
    {
        // 속도 자체가 느리기 때문에 간단한 반복작업에 부적합.
        // AutoResetEvent _available = new AutoResetEvent(true);
        ManualResetEvent _available = new ManualResetEvent(true);

        public void Acquire()
        {
            _available.WaitOne(); // Auto에서는 자동으로 false로 변환
            _available.Reset(); // false로 변환 다만 Manual에서 진행 시 단계가 2단계가 되기 때문에 문제 발생 가능
                                // 즉 하나씩 이동 시에는 Manual이 불필요하지만 다량의 쓰레드가 들어와야할 경우 필요
        }

        public void Release()
        {
            _available.Set(); // true로 변환
        }
    }
    class Program
    {
        static int number = 0;
        static Lock _lock = new Lock();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                number++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                number--;
                _lock.Release();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll();

            Console.WriteLine(number);
        }
    }
}
