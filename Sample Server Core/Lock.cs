using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sample_Server_Core
{
    // 재귀적 락 허용(YES) Lock이 Acquire 됐을 때 그 안에서 다시 Acquire 요청 시 허용 하는지
    // YES | WriteLock -> WriteLock , WriteLock -> ReadLock.
    // 스핀락 (5000번 이후 Yield)

    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        int _flag = EMPTY_FLAG;
        // [Unused(1)] [WriteThread(15)] [ReadCount(16)]
        // Write를 누가 하고 있는지. Read를 얼마나 하고있는지. 
        int _writeCount = 0;

        public void WriteLock()
        {
            // 재귀 허용 시 ID를 비교하여 같은 경우 더 늘려줌
            int curThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == curThreadId)
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock or ReadLock을 하지 않을 때, 경합해서 소유권을 얻음.
            int expected = EMPTY_FLAG;
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, expected) == expected)
                    {
                        _writeCount = 1;
                        return;
                    }
                }
            }
        }

        public void WriteUnLock()
        {
            int curCount = --_writeCount;
            if (curCount == 0) Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            // 이미 WriteLock을 갖고 있다면 ReadLock 부여. 다만 언락 시 순서에 주의
            int curThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == curThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock이 없을 때 ReadCount 증가
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // flag에 READ MASK를 씌우게 되면 WRITE 부분이 자연스럽게 0이 되는데
                    // 이 결과가 flag와 같다면 WRITE가 없는 것이 됨.
                    // 이 후 Read count를 하나 늘려야 하는데 read count는 가장 뒷 자리이므로 그냥 +1
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected) return;
                }

                Thread.Yield();
            }

        }

        public void ReadUnLock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
