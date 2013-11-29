using System.Threading;

namespace Hazelcast.Net.Ext
{
    internal class AtomicReferenceArray<T> where T : class
    {
        private readonly T[] array;

        public AtomicReferenceArray(int size)
        {
            array = new T[size];
        }

        public bool CompareAndSet(int slot, T expect, T update)
        {
            return (Interlocked.CompareExchange(ref array[slot], update, expect) == expect);
        }

        public T Get(int n)
        {
            return array[n];
        }

        public int Length()
        {
            return array.Length;
        }
    }
}