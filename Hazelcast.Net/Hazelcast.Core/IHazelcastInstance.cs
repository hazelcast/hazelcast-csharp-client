// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>Hazelcast client instance.</summary>
    /// <remarks>
    /// Each instance is a member and/or client in a Hazelcast cluster.
    /// When you want to use Hazelcast's distributed data structures, you must first create an instance.
    ///
    /// Instances should be disposed explicitly. If you don't shut it down explicitly,
    /// it will continue to run and even connect to another live member if the one it was connected fails.
    /// 
    /// Each instance has its own socket, threads.
    /// </remarks>
    public interface IHazelcastInstance : IDisposable
    {
        /// <summary>Returns the name of this Hazelcast instance</summary>
        /// <returns>name of this Hazelcast instance</returns>
        string Name { get; }

        /// <summary>Returns unique uuid for this client instance</summary>
        /// <value>unique uuid for this client instance</value>
        Guid ClientGuid { get; }

        /// <summary>Returns the ICluster that this Hazelcast instance is part of.</summary>
        /// <remarks>
        ///     ICluster interface allows you to Add listener for membership
        ///     events and learn more about the cluster that this Hazelcast
        ///     instance is part of.
        /// </remarks>
        /// <returns>cluster that this Hazelcast instance is part of</returns>
        ICluster Cluster { get; }

        /// <summary>Returns the lifecycle service for this instance.</summary>
        /// <remarks>
        ///     Returns the lifecycle service for this instance. ILifecycleService allows you
        ///     to shutdown this IHazelcastInstance and listen for
        ///     the lifecycle events.
        /// </remarks>
        /// <returns>lifecycle service</returns>
        ILifecycleService LifecycleService { get; }
        
        /// <summary>
        /// Returns the partition service of this Hazelcast instance.
        /// </summary>
        IPartitionService PartitionService { get; }

        /// <summary>
        ///     Adds a Distributed Object listener which will be notified when a
        ///     new
        ///     <see cref="IDistributedObject">IDistributedObject</see>
        ///     will be created or destroyed.
        /// </summary>
        /// <param name="distributedObjectListener">instance listener</param>
        /// <returns>returns registration id.</returns>
        Guid AddDistributedObjectListener(IDistributedObjectListener distributedObjectListener);

        /// <summary>Removes the specified Distributed Object listener.</summary>
        /// <remarks>
        ///     Removes the specified Distributed Object listener. Returns silently
        ///     if specified instance listener doesn't exist.
        /// </remarks>
        /// <param name="registrationId">Id of listener registration.</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveDistributedObjectListener(Guid registrationId);

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

        /// <summary>
        /// PN (Positive-Negative) CRDT counter.
        /// </summary>
        /// <param name="name">name of the IPNCounter proxy</param>
        /// <returns>IPNCounter proxy for the given name</returns>
        IPNCounter GetPNCounter(string name);

        /// <summary>Returns the distributed list instance with the specified name.</summary>
        /// <remarks>
        ///     Returns the distributed list instance with the specified name.
        ///     Index based operations on the list are not supported.
        /// </remarks>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="name">name of the distributed list</param>
        /// <returns>distributed list instance with the specified name</returns>
        IHList<T> GetList<T>(string name);

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

        /// <summary>Creates a new ITransactionContext associated with the current thread using default options.</summary>
        /// <remarks>Creates a new ITransactionContext associated with the current thread using default options.</remarks>
        /// <returns>new ITransactionContext</returns>
        ITransactionContext NewTransactionContext();

        /// <summary>Creates a new ITransactionContext associated with the current thread with given options.</summary>
        /// <remarks>Creates a new ITransactionContext associated with the current thread with given options.</remarks>
        /// <param name="options">options for this transaction</param>
        /// <returns>new ITransactionContext</returns>
        ITransactionContext NewTransactionContext(TransactionOptions options);

        /// <summary>Shuts down this IHazelcastInstance.</summary>
        /// <remarks>
        ///     Shuts down this IHazelcastInstance. For more information see
        ///     <see cref="Hazelcast.Core.ILifecycleService"/>.
        /// </remarks>
        void Shutdown();
    }
}