using System.Collections.Generic;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>Transactional implementation of MultiMap</summary>
    public interface ITransactionalMultiMap<K, V> : ITransactionalObject
    {

        bool Put(K key, V value);

        ICollection<V> Get(K key);

        bool Remove(object key, object value);

        ICollection<V> Remove(object key);

        int ValueCount(K key);

        int Size();
    }
}