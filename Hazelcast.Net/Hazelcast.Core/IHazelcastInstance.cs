using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>Hazelcast instance.</summary>
    /// <remarks>
    ///     Hazelcast instance. Each Hazelcast instance is a member (node) in a cluster.
    ///     Multiple Hazelcast instances can be created on a JVM.
    ///     Each Hazelcast instance has its own socket, threads.
    /// </remarks>
    public interface IHazelcastInstance
    {
        /// <summary>Returns the name of this Hazelcast instance</summary>
        /// <returns>name of this Hazelcast instance</returns>
        string GetName();

        /// <summary>Returns the distributed queue instance with the specified name.</summary>
        /// <remarks>Returns the distributed queue instance with the specified name.</remarks>
        /// <param name="name">name of the distributed queue</param>
        /// <returns>distributed queue instance with the specified name</returns>
        IQueue<E> GetQueue<E>(string name);

        /// <summary>Returns the distributed topic instance with the specified name.</summary>
        /// <remarks>Returns the distributed topic instance with the specified name.</remarks>
        /// <param name="name">name of the distributed topic</param>
        /// <returns>distributed topic instance with the specified name</returns>
        ITopic<E> GetTopic<E>(string name);

        /// <summary>Returns the distributed set instance with the specified name.</summary>
        /// <remarks>Returns the distributed set instance with the specified name.</remarks>
        /// <param name="name">name of the distributed set</param>
        /// <returns>distributed set instance with the specified name</returns>
        IHazelcastSet<E> GetSet<E>(string name);

        /// <summary>Returns the distributed list instance with the specified name.</summary>
        /// <remarks>
        ///     Returns the distributed list instance with the specified name.
        ///     Index based operations on the list are not supported.
        /// </remarks>
        /// <param name="name">name of the distributed list</param>
        /// <returns>distributed list instance with the specified name</returns>
        IHazelcastList<E> GetList<E>(string name);

        /// <summary>Returns the distributed map instance with the specified name.</summary>
        /// <remarks>Returns the distributed map instance with the specified name.</remarks>
        /// <param name="name">name of the distributed map</param>
        /// <returns>distributed map instance with the specified name</returns>
        IHazelcastMap<K, V> GetMap<K, V>(string name);

        /// <summary>Returns the distributed multimap instance with the specified name.</summary>
        /// <remarks>Returns the distributed multimap instance with the specified name.</remarks>
        /// <param name="name">name of the distributed multimap</param>
        /// <returns>distributed multimap instance with the specified name</returns>
        IMultiMap<K, V> GetMultiMap<K, V>(string name);

        /// <summary>Returns the distributed lock instance for the specified key object.</summary>
        /// <remarks>
        ///     Returns the distributed lock instance for the specified key object.
        ///     The specified object is considered to be the key for this lock.
        ///     So keys are considered equals cluster-wide as long as
        ///     they are serialized to the same byte array such as String, long,
        ///     Integer.
        ///     <p />
        ///     Locks are fail-safe. If a member holds a lock and some of the
        ///     members go down, cluster will keep your locks safe and available.
        ///     Moreover, when a member leaves the cluster, all the locks acquired
        ///     by this dead member will be removed so that these locks can be
        ///     available for live members immediately.
        ///     <pre>
        ///         Lock lock = hazelcastInstance.getLock("PROCESS_LOCK");
        ///         lock.lock();
        ///         try {
        ///         // process
        ///         } finally {
        ///         lock.unlock();
        ///         }
        ///     </pre>
        /// </remarks>
        /// <param name="key">key of the lock instance</param>
        /// <returns>distributed lock instance for the specified key.</returns>
        ILock GetLock(string key);

        //[Obsolete(@"will be removed in Hazelcast 3.2. Use GetLock(string) instead.")]
        //ILock GetLock(object key);

        /// <summary>Returns the ICluster that this Hazelcast instance is part of.</summary>
        /// <remarks>
        ///     Returns the ICluster that this Hazelcast instance is part of.
        ///     ICluster interface allows you to add listener for membership
        ///     events and learn more about the cluster that this Hazelcast
        ///     instance is part of.
        /// </remarks>
        /// <returns>cluster that this Hazelcast instance is part of</returns>
        ICluster GetCluster();

        /// <summary>Returns the local Endpoint which this IHazelcastInstance is belongs to.</summary>
        /// <remarks>
        ///     Returns the local Endpoint which this IHazelcastInstance is belongs to.
        ///     <p />
        ///     Returned endpoint will be a
        ///     <see cref="IMember">IMember</see>
        ///     instance for cluster nodes
        ///     and a
        ///     <see cref="IClient">IClient</see>
        ///     instance for clients.
        /// </remarks>
        /// <seealso cref="IMember">IMember</seealso>
        /// <seealso cref="IClient">IClient</seealso>
        /// <returns>local endpoint</returns>
        IClient GetLocalEndpoint();

        /// <summary>
        ///     Returns the distributed executor service for the given
        ///     name.
        /// </summary>
        /// <remarks>
        ///     Returns the distributed executor service for the given
        ///     name.
        ///     Executor service enables you to run your <tt>Runnable</tt>s and <tt>Callable</tt>s
        ///     on the Hazelcast cluster.
        ///     <p />
        ///     <p>
        ///         <b>Note:</b> Note that it don't support invokeAll/Any
        ///         and don't have standard shutdown behavior
        ///     </p>
        /// </remarks>
        /// <param name="name">name of the executor service</param>
        /// <returns>executor service for the given name</returns>
        IExecutorService GetExecutorService(string name);

        /// <summary>
        ///     Executes given transactional task in current thread using default options
        ///     and returns the result of the task.
        /// </summary>
        /// <remarks>
        ///     Executes given transactional task in current thread using default options
        ///     and returns the result of the task.
        /// </remarks>
        /// <param name="task">task to be executed</param>
        /// <?></?>
        /// 
        /// <returns>returns result of transactional task</returns>
        /// <exception cref="Hazelcast.Transaction.TransactionException">if an error occurs during transaction.</exception>
        T ExecuteTransaction<T>(ITransactionalTask<T> task);

        /// <summary>
        ///     Executes given transactional task in current thread using given options
        ///     and returns the result of the task.
        /// </summary>
        /// <remarks>
        ///     Executes given transactional task in current thread using given options
        ///     and returns the result of the task.
        /// </remarks>
        /// <param name="options">options for this transactional task</param>
        /// <param name="task">task to be executed</param>
        /// <?></?>
        /// 
        /// <returns>returns result of transactional task</returns>
        /// <exception cref="Hazelcast.Transaction.TransactionException">if an error occurs during transaction.</exception>
        T ExecuteTransaction<T>(TransactionOptions options, ITransactionalTask<T> task);

        /// <summary>Creates a new ITransactionContext associated with the current thread using default options.</summary>
        /// <remarks>Creates a new ITransactionContext associated with the current thread using default options.</remarks>
        /// <returns>new ITransactionContext</returns>
        ITransactionContext NewTransactionContext();

        /// <summary>Creates a new ITransactionContext associated with the current thread with given options.</summary>
        /// <remarks>Creates a new ITransactionContext associated with the current thread with given options.</remarks>
        /// <param name="options">options for this transaction</param>
        /// <returns>new ITransactionContext</returns>
        ITransactionContext NewTransactionContext(TransactionOptions options);

        /// <summary>Creates cluster-wide unique IDs.</summary>
        /// <remarks>
        ///     Creates cluster-wide unique IDs. Generated IDs are long type primitive values
        ///     between <tt>0</tt> and <tt>Long.MAX_VALUE</tt> . Id generation occurs almost at the speed of
        ///     <tt>AtomicLong.incrementAndGet()</tt> . Generated IDs are unique during the life
        ///     cycle of the cluster. If the entire cluster is restarted, IDs start from <tt>0</tt> again.
        /// </remarks>
        /// <param name="name">name of the IIdGenerator</param>
        /// <returns>IIdGenerator for the given name</returns>
        IIdGenerator GetIdGenerator(string name);

        /// <summary>Creates cluster-wide atomic long.</summary>
        /// <remarks>
        ///     Creates cluster-wide atomic long. Hazelcast IAtomicLong is distributed
        ///     implementation of <tt>java.util.concurrent.atomic.AtomicLong</tt>.
        /// </remarks>
        /// <param name="name">name of the IAtomicLong proxy</param>
        /// <returns>IAtomicLong proxy for the given name</returns>
        IAtomicLong GetAtomicLong(string name);

        /// <summary>Creates cluster-wide CountDownLatch.</summary>
        /// <remarks>
        ///     Creates cluster-wide CountDownLatch. Hazelcast ICountDownLatch is distributed
        ///     implementation of <tt>java.util.concurrent.CountDownLatch</tt>.
        /// </remarks>
        /// <param name="name">name of the ICountDownLatch proxy</param>
        /// <returns>ICountDownLatch proxy for the given name</returns>
        ICountDownLatch GetCountDownLatch(string name);

        /// <summary>Creates cluster-wide semaphore.</summary>
        /// <remarks>
        ///     Creates cluster-wide semaphore. Hazelcast ISemaphore is distributed
        ///     implementation of <tt>java.util.concurrent.Semaphore</tt>.
        /// </remarks>
        /// <param name="name">name of the ISemaphore proxy</param>
        /// <returns>ISemaphore proxy for the given name</returns>
        ISemaphore GetSemaphore(string name);

        /// <summary>
        ///     Returns all
        ///     <see cref="IDistributedObject">IDistributedObject</see>
        ///     's such as; queue, map, set, list, topic, lock, multimap.
        /// </summary>
        /// <returns>the collection of instances created by Hazelcast.</returns>
        ICollection<IDistributedObject> GetDistributedObjects();

        /// <summary>
        ///     Adds a Distributed Object listener which will be notified when a
        ///     new
        ///     <see cref="IDistributedObject">IDistributedObject</see>
        ///     will be created or destroyed.
        /// </summary>
        /// <param name="distributedObjectListener">instance listener</param>
        /// <returns>returns registration id.</returns>
        string AddDistributedObjectListener(IDistributedObjectListener distributedObjectListener);

        /// <summary>Removes the specified Distributed Object listener.</summary>
        /// <remarks>
        ///     Removes the specified Distributed Object listener. Returns silently
        ///     if specified instance listener doesn't exist.
        /// </remarks>
        /// <param name="registrationId">Id of listener registration.</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveDistributedObjectListener(string registrationId);

        //    /**
        //     * Returns the configuration of this Hazelcast instance.
        //     *
        //     * @return configuration of this Hazelcast instance
        //     */
        //    Config getConfig();
        //    /**
        //     * Returns the partition service of this Hazelcast instance.
        //     * PartitionService allows you to introspect current partitions in the
        //     * cluster, partition owner members and listen for partition migration events.
        //     *
        //     * @return partition service
        //     */
        //    PartitionService getPartitionService();
        /// <summary>Returns the client service of this Hazelcast instance.</summary>
        /// <remarks>
        ///     Returns the client service of this Hazelcast instance.
        ///     IClient service allows you to get information about connected clients.
        /// </remarks>
        /// <returns>the IClientService.</returns>
        IClientService GetClientService();

        //    /**
        //     * Returns the logging service of this Hazelcast instance.
        //     * LoggingService allows you to listen for LogEvents
        //     * generated by Hazelcast runtime. You can log the events somewhere
        //     * or take action base on the message.
        //     *
        //     * @return logging service
        //     */
        //    LoggingService getLoggingService();
        /// <summary>Returns the lifecycle service for this instance.</summary>
        /// <remarks>
        ///     Returns the lifecycle service for this instance. ILifecycleService allows you
        ///     to shutdown this IHazelcastInstance and listen for
        ///     the lifecycle events.
        /// </remarks>
        /// <returns>lifecycle service</returns>
        ILifecycleService GetLifecycleService();

        ///// <param name="serviceName">name of the service</param>
        ///// <param name="id">identifier of the object</param>
        ///// <?></?>
        ///// 
        ///// <returns>IDistributedObject created by the service</returns>
        //[Obsolete(@"use GetDistributedObject{T}(string, string) instead.")]
        //T GetDistributedObject<T>(string serviceName, object id) where T : IDistributedObject;

        /// <param name="serviceName">name of the service</param>
        /// <param name="name">name of the object</param>
        /// <?></?>
        /// 
        /// <returns>IDistributedObject created by the service</returns>
        T GetDistributedObject<T>(string serviceName, string name) where T : IDistributedObject;

        /// <summary>Returns a ConcurrentMap that can be used to add user-context to the IHazelcastInstance.</summary>
        /// <remarks>
        ///     Returns a ConcurrentMap that can be used to add user-context to the IHazelcastInstance. This can be used
        ///     to store dependencies that otherwise are hard to obtain. IHazelcastInstance can be
        ///     obtained by implementing IHazelcastInstanceAware interface when submitting a Runnable/Callable to
        ///     Hazelcast ExecutorService. By storing the dependencies in the user-context, they can be retrieved as soon
        ///     as you have a reference to the IHazelcastInstance.
        ///     <p />
        ///     This structure is purely local and Hazelcast remains agnostic abouts its content.
        /// </remarks>
        /// <returns>the user context.</returns>
        ConcurrentDictionary<string, object> GetUserContext();

        /// <summary>Shuts down this IHazelcastInstance.</summary>
        /// <remarks>
        ///     Shuts down this IHazelcastInstance. For more information see
        ///     <see cref="ILifecycleService.Shutdown()">ILifecycleService.Shutdown()</see>
        ///     .
        /// </remarks>
        void Shutdown();
    }
}