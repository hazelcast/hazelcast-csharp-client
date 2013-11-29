using System.Threading;

namespace Hazelcast.Net.Ext
{
    public class AtomicReference<T> where T : class
    {
        private T val;

        public AtomicReference()
        {
        }

        public AtomicReference(T val)
        {
            this.val = val;
        }

        public bool CompareAndSet(T expect, T update)
        {
            return (Interlocked.CompareExchange(ref val, update, expect) == expect);
        }

        public T Get()
        {
            return val;
        }

        public void Set(T t)
        {
            val = t;
        }
    }
}