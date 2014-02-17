using System.Threading;

namespace Hazelcast.Net.Ext
{
    internal class AtomicBoolean
    {
        private volatile int value;

        public AtomicBoolean()
        {
        }

        public AtomicBoolean(bool initialValue)
        {
            value = initialValue ? 1 : 0;
        }

        public bool CompareAndSet(bool expect, bool update)
        {
            int e = expect ? 1 : 0;
            int u = update ? 1 : 0;
            return (Interlocked.CompareExchange(ref value, u, e) == e);
        }

        public bool Get()
        {
            return value != 0;
        }

        public bool weakCompareAndSet(bool expect, bool update)
        {
            return CompareAndSet(expect, update);
        }


        public void Set(bool newValue)
        {
            value = newValue ? 1 : 0;
        }

        public void LazySet(bool newValue)
        {
            int v = newValue ? 1 : 0;
        }

        public bool GetAndSet(bool newValue)
        {
            for (;;)
            {
                bool current = Get();
                if (CompareAndSet(current, newValue))
                    return current;
            }
        }
    }
}