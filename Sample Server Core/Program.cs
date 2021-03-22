using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample_Server_Core
{
    class SpinLock
    {
        volatile int _locked = 0; 

        public void Acquire()
        {
            while (true)
            {
                int expected = 0;
                int desired = 1;
                if(Interlocked.CompareExchange(ref _locked, desired, expected) == expected) break;

                // Thread.Sleep(1); // N ms 만큼 대기
                // Thread.Sleep(0); // 우선순위에 따라서 양보
                Thread.Yield();  // 실행 가능한 쓰레드에게 양보
            }
        }

        public void Release()
        {
            _locked = 0;
        }
    }
    class Program
    {
        static int number = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 1000000000; i++)
            {
                _lock.Acquire();
                number++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000000; i++)
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
