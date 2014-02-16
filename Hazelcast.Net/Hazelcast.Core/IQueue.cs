using System.Collections.Generic;
using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    /// <summary>Concurrent, blocking, distributed, observable queue.</summary>
    public interface IQueue<E> : IHCollection<E>
    {
        bool Contains(object o);
        bool Offer(E e);
        bool Offer(E e, long timeout, TimeUnit unit);
        E Peek();
        E Poll();
        E Poll(long timeout, TimeUnit unit);
        void Put(E e);
        E Take();
        bool Remove(object o);
        E Remove();
        int DrainTo<T>(ICollection<T> c) where T : E;
        int DrainTo<T>(ICollection<T> c, int maxElements) where T : E;
        int RemainingCapacity();
        E Element();

    }
}