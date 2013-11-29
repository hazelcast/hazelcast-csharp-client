using System;
using System.Collections.Generic;
using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    public interface IBlockingQueue<E>
    {
        bool Add(E e);

        bool Offer(E e);


        E Remove();


        E Poll();


        E Element();

        E Peek();

        void Put(E e);


        bool Offer(E e, long timeout, TimeUnit unit);

        E Take();


        E Poll(long timeout, TimeUnit unit);


        int RemainingCapacity();


        bool Remove(Object o);

        bool Contains(Object o);


        int DrainTo<T>(ICollection<T> c) where T : E;


        int DrainTo<T>(ICollection<T> c, int maxElements) where T : E;
    }
}