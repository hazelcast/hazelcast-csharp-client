using Hazelcast.Core;

namespace Hazelcast.Transaction
{
    /// <summary>
    ///     Provides a context to access transactional data-structures like the
    ///     <see cref="ITransactionalMap{K,V}">Hazelcast.Core.ITransactionalMap&lt;K, V&gt;</see>
    ///     .
    /// </summary>
    public interface ITransactionalTaskContext
    {
        /// <summary>Returns the transactional distributed map instance with the specified name.</summary>
        /// <remarks>Returns the transactional distributed map instance with the specified name.</remarks>
        /// <param name="name">name of the distributed map</param>
        /// <returns>transactional distributed map instance with the specified name</returns>
        ITransactionalMap<K, V> GetMap<K, V>(string name);

        /// <summary>Returns the transactional queue instance with the specified name.</summary>
        /// <remarks>Returns the transactional queue instance with the specified name.</remarks>
        /// <param name="name">name of the queue</param>
        /// <returns>transactional queue instance with the specified name</returns>
        ITransactionalQueue<E> GetQueue<E>(string name);

        /// <summary>Returns the transactional multimap instance with the specified name.</summary>
        /// <remarks>Returns the transactional multimap instance with the specified name.</remarks>
        /// <param name="name">name of the multimap</param>
        /// <returns>transactional multimap instance with the specified name</returns>
        ITransactionalMultiMap<K, V> GetMultiMap<K, V>(string name);

        /// <summary>Returns the transactional list instance with the specified name.</summary>
        /// <remarks>Returns the transactional list instance with the specified name.</remarks>
        /// <param name="name">name of the list</param>
        /// <returns>transactional list instance with the specified name</returns>
        ITransactionalList<E> GetList<E>(string name);

        /// <summary>Returns the transactional set instance with the specified name.</summary>
        /// <remarks>Returns the transactional set instance with the specified name.</remarks>
        /// <param name="name">name of the set</param>
        /// <returns>transactional set instance with the specified name</returns>
        ITransactionalSet<E> GetSet<E>(string name);

        T GetTransactionalObject<T>(string serviceName, string name) where T : ITransactionalObject;
    }
}