using System.Runtime.CompilerServices;
using System.Threading;

namespace Hazelcast.Net.Ext
{
    internal class AtomicLong
    {
        private long val;

        public AtomicLong()
        {
        }

        public AtomicLong(long val)
        {
            this.val = val;
        }

        public long AddAndGet(long addval)
        {
            return Interlocked.Add(ref val, addval);
        }

        public bool CompareAndSet(long expect, long update)
        {
            return (Interlocked.CompareExchange(ref val, update, expect) == expect);
        }

        public long DecrementAndGet()
        {
            return Interlocked.Decrement(ref val);
        }

        public long Get()
        {
            return val;
        }

        public void Set(long newValue)
        {
            val = newValue;
        }

        public long IncrementAndGet()
        {
            return Interlocked.Increment(ref val);
        }

        public long GetAndSet(int i)
        {
            return Interlocked.Exchange(ref val, i);
        }
    }
}