using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample_Server_Core
{
    class SessionManager
    {
        static object _lock = new object();

        public static void Test()
        {
            lock (_lock)
            {
                UserManager.TestUser();
            }
        }

        public static void TestSession()
        {
            lock(_lock)
            {

            }
        }
    }
    class UserManager
    {
        static object _lock = new object();

        public static void Test()
        {
            lock (_lock)
            {
                SessionManager.TestSession();
            }
        }

        public static void TestUser()
        {
            lock (_lock)
            {

            }
        }
    }
    class Program
    {
        static int number = 0;

        static void Thread_1()
        {
            for (int i = 0; i < 1000000; i++)
            {
                SessionManager.Test();
            }    
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000; i++)
            {
                UserManager.Test();
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
