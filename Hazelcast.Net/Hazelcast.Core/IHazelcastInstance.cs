// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>Hazelcast instance.</summary>
    /// <remarks>
    ///     Hazelcast instance. Each Hazelcast instance is a member (node) in a cluster.
    ///     Multiple Hazelcast instances can be created.
    ///     Each Hazelcast instance has its own socket, threads.
    /// </remarks>
    public interface IHazelcastInstance
    {
        /// <summary>
        ///     Adds a Distributed Object listener which will be notified when a
        ///     new
        ///     <see cref="IDistributedObject">IDistributedObject</see>
        ///     will be created or destroyed.
        /// </summary>
        /// <param name="distributedObjectListener">instance listener</param>
        /// <returns>returns registration id.</returns>
        string AddDistributedObjectListener(IDistributedObjectListener distributedObjectListener);

        /// <summary>Creates cluster-wide atomic long.</summary>
        /// <remarks>
        ///     Creates cluster-wide atomic long. Hazelcast IAtomicLong is distributed
        ///     implementation of <c>Interlocked</c>.
        /// </remarks>
        /// <param name="name">name of the IAtomicLong proxy</param>
        /// <returns>IAtomicLong proxy for the given name</returns>
        IAtomicLong GetAtomicLong(string name);

        /// <summary>Returns the client service of this Hazelcast instance.</summary>
        /// <remarks>
        ///     Returns the client service of this Hazelcast instance.
        ///     IClient service allows you to get information about connected clients.
        /// </remarks>
        /// <returns>the IClientService.</returns>
        IClientService GetClientService();

        /// <summary>Returns the ICluster that this Hazelcast instance is part of.</summary>
        /// <remarks>
        ///     Returns the ICluster that this Hazelcast instance is part of.
        ///     ICluster interface allows you to add listener for membership
        ///     events and learn more about the cluster that this Hazelcast
        ///     instance is part of.
        /// </remarks>
        /// <returns>cluster that this Hazelcast instance is part of</returns>
        ICluster GetCluster();

        /// <summary>Creates cluster-wide CountDownLatch.</summary>
        /// <remarks>
        ///     Creates cluster-wide CountDownLatch. Hazelcast ICountDownLatch is distributed
        ///     implementation of <see cref="System.Threading.CountdownEvent"/>.
        /// </remarks>
        /// <param name="name">name of the ICountDownLatch proxy</param>
        /// <returns>ICountDownLatch proxy for the given name</returns>
        ICountDownLatch GetCountDownLatch(string name);

        /// <summary>
        /// PN (Positive-Negative) CRDT counter.
        /// </summary>
        /// <param name="name">name of the IPNCounter proxy</param>
        /// <returns>IPNCounter proxy for the given name</returns>
        IPNCounter GetPNCounter(string name);

        /// <param name="serviceName">name of the service</param>
        /// <param name="name">name of the object</param>
        /// <returns>IDistributedObject created by the service</returns>
        T GetDistributedObject<T>(string serviceName, string name) where T : IDistributedObject;

        /// <summary>
        ///     Returns all
        ///     <see cref="IDistributedObject">IDistributedObject</see>
        ///     's such as; queue, map, set, list, topic, lock, multimap.
        /// </summary>
        /// <returns>the collection of instances created by Hazelcast.</returns>
        ICollection<IDistributedObject> GetDistributedObjects();

        /// <summary>Creates cluster-wide unique IDs.</summary>
        /// <remarks>
        ///     Creates cluster-wide unique IDs. Generated IDs are long type primitive values
        ///     between <c>0</c> and <c>Int64.MaxValue</c> . Id generation occurs almost at the speed of
        ///     <c>Interlocked.Increment(long)</c> . Generated IDs are unique during the life
        ///     cycle of the cluster. If the entire cluster is restarted, IDs start from <c>0</c> again.
        /// </remarks>
        /// <param name="name">name of the IIdGenerator</param>
        /// <returns>IIdGenerator for the given name</returns>
        IIdGenerator GetIdGenerator(string name);

        /// <summary>Returns the lifecycle service for this instance.</summary>
        /// <remarks>
        ///     Returns the lifecycle service for this instance. ILifecycleService allows you
        ///     to shutdown this IHazelcastInstance and listen for
        ///     the lifecycle events.
        /// </remarks>
        /// <returns>lifecycle service</returns>
        ILifecycleService GetLifecycleService();

        /// <summary>Returns the distributed list instance with the specified name.</summary>
        /// <remarks>
        ///     Returns the distributed list instance with the specified name.
        ///     Index based operations on the list are not supported.
        /// </remarks>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="name">name of the distributed list</param>
        /// <returns>distributed list instance with the specified name</returns>
        IHList<T> GetList<T>(string name);

        /// <summary>Returns the local Endpoint which this IHazelcastInstance is belongs to.</summary>
        /// <returns>local endpoint</returns>
        IEndpoint GetLocalEndpoint();

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
        /// </remarks>
        /// <param name="key">key of the lock instance</param>
        /// <returns>distributed lock instance for the specified key.</returns>
        /// <example>
        /// 
        /// <code >
        ///Lock lock = hazelcastInstance.GetLock("PROCESS_LOCK");
        ///lock.lock();
        ///try
        ///{
        /// // process
        ///} 
        ///finally
        ///{
        ///    lock.unlock();
        ///}
        /// </code>
        /// </example>
        ILock GetLock(string key);

        /// <summary>
        /// Returns the distributed map instance with the specified name.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the map</typeparam>
        /// <typeparam name="TValue">The type of the values in the map</typeparam>
        /// <param name="name">name of the distributed map</param>
        /// <returns>distributed map instance with the specified name</returns>
        /// <remarks>Returns the distributed map instance with the specified name.</remarks>
        /// <example>
        /// <code>
        ///     var myMap = hazelcastInstance.GetMap&lt;string,int&gt;("theMap");
        ///     myMap.Put("item1",10);
        ///     int value=myMap.Get("item1");
        ///     //value==10
        /// </code>
        /// </example>
        IMap<TKey, TValue> GetMap<TKey, TValue>(string name);

        /// <summary>Returns the distributed multimap instance with the specified name.</summary>
        /// <remarks>Returns the distributed multimap instance with the specified name.</remarks>
        /// <typeparam name="TKey">The type of the keys in the multimap</typeparam>
        /// <typeparam name="TValue">The type of the values in the multimap</typeparam>
        /// <param name="name">name of the distributed multimap</param>
        /// <returns>distributed multimap instance with the specified name</returns>
        IMultiMap<TKey, TValue> GetMultiMap<TKey, TValue>(string name);

        /// <summary>
        /// Returns the replicated map instance with the specified name.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the map</typeparam>
        /// <typeparam name="TValue">The type of the values in the map</typeparam>
        /// <param name="name">name of the distributed map</param>
        /// <returns>distributed map instance with the specified name</returns>
        IReplicatedMap<TKey, TValue> GetReplicatedMap<TKey, TValue>(string name);

        /// <summary>Returns the name of this Hazelcast instance</summary>
        /// <returns>name of this Hazelcast instance</returns>
        string GetName();

        /// <summary>
        /// Returns the distributed queue instance with the specified name.
        /// </summary>
        /// <typeparam name="T">The type of elements in the queue.</typeparam>
        /// <param name="name">name of the distributed queue</param>
        /// <returns>distributed queue instance with the specified name</returns>
        /// <remarks>Returns the distributed queue instance with the specified name.</remarks>
        IQueue<T> GetQueue<T>(string name);

        /// <summary>Returns the distributed Ringbuffer instance with the specified name.</summary>
        /// <param name="name">name of the distributed Ringbuffer</param>
        /// <returns>distributed RingBuffer instance with the specified name</returns>
        IRingbuffer<T> GetRingbuffer<T>(string name);

        /// <summary>Creates cluster-wide semaphore.</summary>
        /// <remarks>
        ///     Creates cluster-wide semaphore. Hazelcast ISemaphore is distributed
        ///     implementation of <see cref="Semaphore"/>.
        /// </remarks>
        /// <param name="name">name of the ISemaphore proxy</param>
        /// <returns>ISemaphore proxy for the given name</returns>
        ISemaphore GetSemaphore(string name);

        /// <summary>Returns the distributed set instance with the specified name.</summary>
        /// <remarks>Returns the distributed set instance with the specified name.</remarks>
        /// <typeparam name="T">The type of elements in the set</typeparam>
        /// <param name="name">name of the distributed set</param>
        /// <returns>distributed set instance with the specified name</returns>
        IHSet<T> GetSet<T>(string name);

        /// <summary>Returns the distributed topic instance with the specified name.</summary>
        /// <remarks>Returns the distributed topic instance with the specified name.</remarks>
        /// <typeparam name="T">The type of elements in the Topic</typeparam>
        /// <param name="name">name of the distributed topic</param>
        /// <returns>distributed topic instance with the specified name</returns>
        ITopic<T> GetTopic<T>(string name);

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

        ///// <summary>
        /////     Returns the distributed executor service for the given
        /////     name.
        ///// </summary>
        ///// <remarks>
        /////     Returns the distributed executor service for the given
        /////     name.
        /////     Executor service enables you to run your <tt>Runnable</tt>s and <tt>Callable</tt>s
        /////     on the Hazelcast cluster.
        /////     <p />
        /////     <p>
        /////         <b>Note:</b> Note that it don't support invokeAll/Any
        /////         and don't have standard shutdown behavior
        /////     </p>
        ///// </remarks>
        ///// <param name="name">name of the executor service</param>
        ///// <returns>executor service for the given name</returns>
        //IExecutorService GetExecutorService(string name);

        /// <summary>Creates a new ITransactionContext associated with the current thread using default options.</summary>
        /// <remarks>Creates a new ITransactionContext associated with the current thread using default options.</remarks>
        /// <returns>new ITransactionContext</returns>
        ITransactionContext NewTransactionContext();

        /// <summary>Creates a new ITransactionContext associated with the current thread with given options.</summary>
        /// <remarks>Creates a new ITransactionContext associated with the current thread with given options.</remarks>
        /// <param name="options">options for this transaction</param>
        /// <returns>new ITransactionContext</returns>
        ITransactionContext NewTransactionContext(TransactionOptions options);

        /// <summary>Removes the specified Distributed Object listener.</summary>
        /// <remarks>
        ///     Removes the specified Distributed Object listener. Returns silently
        ///     if specified instance listener doesn't exist.
        /// </remarks>
        /// <param name="registrationId">Id of listener registration.</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveDistributedObjectListener(string registrationId);

        /// <summary>Shuts down this IHazelcastInstance.</summary>
        /// <remarks>
        ///     Shuts down this IHazelcastInstance. For more information see
        ///     <see cref="Hazelcast.Core.ILifecycleService"/>.
        /// </remarks>
        void Shutdown();
    }
}