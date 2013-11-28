using System.Collections.Generic;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Transactional implementation of
    ///     <see cref="IBaseMap{K,V}">BaseMap&lt;K, V&gt;</see>
    ///     .
    /// </summary>
    /// <seealso cref="IBaseMap{K,V}">BaseMap&lt;K, V&gt;</seealso>
    /// <seealso cref="IMap{K, V}">IMap&lt;K, V&gt;</seealso>
    /// <?></?>
    /// <?></?>
    public interface ITransactionalMap<K, V> : ITransactionalObject, IBaseMap<K, V>
    {
        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.ContainsKey(object)">IMap&lt;K, V&gt;.ContainsKey(object)</see>
        ///     .
        /// </summary>
        /// <seealso cref="IMap{K, V}.ContainsKey(object)">IMap&lt;K, V&gt;.ContainsKey(object)</seealso>
        //bool ContainsKey(object key);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Get(object)">IMap&lt;K, V&gt;.Get(object)</see>
        ///     .
        /// </summary>
        /// <seealso cref="IMap{K, V}.Get(object)">IMap&lt;K, V&gt;.Get(object)</seealso>
        //V Get(object key);

        /// <summary>Locks the key and then gets and returns the value to which the specified key is mapped.</summary>
        /// <remarks>
        ///     Locks the key and then gets and returns the value to which the specified key is mapped. Lock will be released
        ///     at the end of the transaction (either commit or rollback).
        /// </remarks>
        /// <seealso cref="IMap{K, V}.Get(object)">IMap&lt;K, V&gt;.Get(object)</seealso>
        V GetForUpdate(object key);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IBaseMap{K,V}.Size()">BaseMap&lt;K, V&gt;.Size()</see>
        ///     .
        /// </summary>
        /// <seealso cref="IBaseMap{K,V}.Size()">BaseMap&lt;K, V&gt;.Size()</seealso>
        //int Size();

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IBaseMap{K,V}.IsEmpty()">BaseMap&lt;K, V&gt;.IsEmpty()</see>
        ///     .
        /// </summary>
        /// <seealso cref="IBaseMap{K,V}.IsEmpty()">BaseMap&lt;K, V&gt;.IsEmpty()</seealso>
        //bool IsEmpty();

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Put(object, object)">IMap&lt;K, V&gt;.Put(object, object)</see>
        ///     .
        ///     <p />
        ///     The object to be put will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Put(object, object)">IMap&lt;K, V&gt;.Put(object, object)</seealso>
        //V Put(K key, V value);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Set(object, object)">IMap&lt;K, V&gt;.Set(object, object)</see>
        ///     .
        ///     <p />
        ///     The object to be set will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Set(object, object)">IMap&lt;K, V&gt;.Set(object, object)</seealso>
        //void Set(K key, V value);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.PutIfAbsent(object, object)">IMap&lt;K, V&gt;.PutIfAbsent(object, object)</see>
        ///     .
        ///     <p />
        ///     The object to be put will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.PutIfAbsent(object, object)">IMap&lt;K, V&gt;.PutIfAbsent(object, object)</seealso>
        //V PutIfAbsent(K key, V value);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Replace(object, object)">IMap&lt;K, V&gt;.Replace(object, object)</see>
        ///     .
        ///     <p />
        ///     The object to be replaced will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Replace(object, object)">IMap&lt;K, V&gt;.Replace(object, object)</seealso>
        //V Replace(K key, V value);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Replace(object, object, object)">IMap&lt;K, V&gt;.Replace(object, object, object)</see>
        ///     .
        ///     <p />
        ///     The object to be replaced will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Replace(object, object, object)">IMap&lt;K, V&gt;.Replace(object, object, object)</seealso>
        //bool Replace(K key, V oldValue, V newValue);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="Sharpen.Collections.Remove(object)">Sharpen.Collections.Remove(object)</see>
        ///     .
        ///     <p />
        ///     The object to be removed will be removed from only the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="Sharpen.Collections.Remove(object)">Sharpen.Collections.Remove(object)</seealso>
        //V Remove(object key);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Delete(object)">IMap&lt;K, V&gt;.Delete(object)</see>
        ///     .
        ///     <p />
        ///     The object to be deleted will be removed from only the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Delete(object)">IMap&lt;K, V&gt;.Delete(object)</seealso>
        //void Delete(object key);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Remove(object, object)">IMap&lt;K, V&gt;.Remove(object, object)</see>
        ///     .
        ///     <p />
        ///     The object to be removed will be removed from only the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Remove(object, object)">IMap&lt;K, V&gt;.Remove(object, object)</seealso>
        //bool Remove(object key, object value);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Keys()">IMap&lt;K, V&gt;.Keys()</see>
        ///     .
        ///     <p />
        /// </summary>
        /// <seealso cref="IMap{K, V}.Keys()">IMap&lt;K, V&gt;.Keys()</seealso>
        ICollection<K> KeySet();

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.KeySet(IPredicate{K,V})">IMap&lt;K, V&gt;.KeySet(IPredicate&lt;K, V&gt;)</see>
        ///     .
        ///     <p />
        /// </summary>
        /// <seealso cref="IMap{K, V}.KeySet(IPredicate{K,V})">IMap&lt;K, V&gt;.KeySet(IPredicate&lt;K, V&gt;)</seealso>
        //ICollection<K> KeySet(IPredicate<K, V> predicate);

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Values()">IMap&lt;K, V&gt;.Values()</see>
        ///     .
        ///     <p />
        /// </summary>
        /// <seealso cref="IMap{K, V}.Values()">IMap&lt;K, V&gt;.Values()</seealso>
        ICollection<V> Values();

        /// <summary>
        ///     Transactional implementation of
        ///     <see cref="IMap{K, V}.Values(IPredicate{K,V})">IMap&lt;K, V&gt;.Values(IPredicate&lt;K, V&gt;)</see>
        ///     .
        ///     <p />
        /// </summary>
        /// <seealso cref="IMap{K, V}.Values(IPredicate{K,V})">IMap&lt;K, V&gt;.Values(IPredicate&lt;K, V&gt;)</seealso>
        //ICollection<V> Values(IPredicate<K, V> predicate);
    }
}