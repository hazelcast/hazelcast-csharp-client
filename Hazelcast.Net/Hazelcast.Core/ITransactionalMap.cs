using System.Collections.Generic;
using Hazelcast.Net.Ext;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>
    /// Transactional implementation of Map
    /// </summary>
    /// <typeparam name="K">key type</typeparam>
    /// <typeparam name="V">Value type</typeparam>
    public interface ITransactionalMap<K, V> : ITransactionalObject
    {
        V GetForUpdate(object key);
        ICollection<K> KeySet();
        ICollection<V> Values();
        bool ContainsKey(object key);
        V Get(object key);
        V Put(K key, V value);
        V Put(K key, V value, long ttl, TimeUnit timeunit);
        V PutIfAbsent(K key, V value);
        V Replace(K key, V value);
        bool Replace(K key, V oldValue, V newValue);
        void Set(K key, V value);
        V Remove(object key);
        void Delete(object key);
        bool Remove(object key, object value);
        bool IsEmpty();
        int Size();
    }
}