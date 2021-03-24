using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample_Server_Core
{
    class Program
    {
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My name is {Thread.CurrentThread.ManagedThreadId}"; });
        // null일 때 입력해준 function이 작동
        // 만약 함수 없이 메서드에 Value를 직접 변동시켜준다면 같은 쓰레드가 호출되더라도 다시 변경시키는 비효율적인 방식이 됨.

        static void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated;
            if (repeat) 
                Console.WriteLine(ThreadName.Value + " - repeat");
            else
                Console.WriteLine(ThreadName.Value);
        }
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);
        }
    }
}
