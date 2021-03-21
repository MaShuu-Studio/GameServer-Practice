using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample_Server_Core
{
    class Program
    {
        // 실행이 되거나 안되거나.
        // Interlocked가 걸려있으면 다른 함수가 진행이 되지 않아 원자성을 보장해줌.
        // 그만큼 성능면에서 저하.

        static int number = 0;

        static void Thread_1()
        {
            for (int i = 0; i < 1000000; i++)
                Interlocked.Increment(ref number);
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000; i++)
                Interlocked.Decrement(ref number);
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
