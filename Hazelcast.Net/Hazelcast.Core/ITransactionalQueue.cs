using Hazelcast.Net.Ext;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>Transactional implementation of Queue</summary>
    public interface ITransactionalQueue<E> : ITransactionalObject
    {
        bool Offer(E e);
        bool Offer(E e, long timeout, TimeUnit unit);
        E Poll();

        E Poll(long timeout, TimeUnit unit);

        E Peek();

        E Peek(long timeout, TimeUnit unit);

        E Take();

        int Size();
    }
}