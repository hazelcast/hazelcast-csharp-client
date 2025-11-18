// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides the <see cref="ICPSubsystem"/> implementation.
    /// </summary>
    internal class CPSubsystem : ICPSubsystem, IAsyncDisposable
    {
        private readonly Cluster _cluster;
        private readonly SerializationService _serializationService;
        private readonly ConcurrentDictionary<string, IFencedLock> _fencedLocks = new ConcurrentDictionary<string, IFencedLock>();
        private readonly ILoggerFactory _loggerFactory;

        // ReSharper disable once InconsistentNaming - internal for tests
        internal readonly CPSessionManager _cpSubsystemSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="CPSubsystem"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public CPSubsystem(Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster;
            _serializationService = serializationService;
            _cpSubsystemSession = new CPSessionManager(cluster);
            _loggerFactory = loggerFactory;
        }

        // NOTES
        //
        // Java CP objects are managed by CPSubsystemImpl and created through ClientRaftProxyFactory
        // which is a simplified factory, which does not cache AtomicLong, AtomicRef, and CountDownLatch,
        // and sort-of caches (?) FencedLock and Semaphore.
        //
        // These objects are therefore IDistributedObject but *not* DistributedObjectBase, and *not*
        // managed by the DistributedObjectFactory.
        //
        // They are destroyed via ClientProxy.destroy, which is getContext().getProxyManager().destroyProxy(this),
        // which means they are destroyed by ProxyManager aka DistributedObjectFactory, which would try to
        // remove them from cache (always missing) and end up doing proxy.destroyLocally() which eventually
        // calls into the object's onDestroy() method.
        //
        // But... this is convoluted? For now, our objects inherit from CPObjectBase which is simpler than
        // DistributedObjectBase, they do not hit DistributedObjectFactory at all, and implement their
        // own destroy method.

        /// <inheritdoc />
        public async Task<ISemaphore> GetSemaphore(string name)
        {
            var (groupName, objectName, fullName) = ParseName(name);
            var groupId = await GetGroupIdAsync(fullName).CfAwait();
            var requestMessage = SemaphoreGetSemaphoreTypeCodec.EncodeRequest(objectName);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var noSession = SemaphoreGetSemaphoreTypeCodec.DecodeResponse(responseMessage).Response;

            return noSession
                ? new SessionLessSemaphore(objectName, groupId, _cluster, _serializationService, _cpSubsystemSession)
                : new SessionAwareSemaphore(objectName, groupId, _cluster, _serializationService, _cpSubsystemSession);
        }

        /// <inheritdoc />
        public async Task<IAtomicLong> GetAtomicLongAsync(string name)
        {
            var (groupName, objectName, fullName) = ParseName(name);
            var groupId = await GetGroupIdAsync(fullName).CfAwait();

            return new AtomicLong(objectName, groupId, _cluster, _serializationService);
        }

        /// <inheritdoc />
        public async Task<IAtomicReference<T>> GetAtomicReferenceAsync<T>(string name)
        {
            var (groupName, objectName, fullName) = ParseName(name);
            var groupId = await GetGroupIdAsync(fullName).CfAwait();

            return new AtomicReference<T>(objectName, groupId, _cluster, _serializationService);
        }

        /// <inheritdoc />
        public async Task<IFencedLock> GetLockAsync(string name)
        {
            var (groupName, objectName, fullName) = ParseName(name);
            var groupId = await GetGroupIdAsync(fullName).CfAwait();

            // note: make sure to use the fully qualified fullName as a dictionary key

            // the code we use is an exact match of the Java code
            // TODO: think about simplifying with the commented code below
            // TODO: make sure there is no race condition here
            /*
            while (true)
            {
                var fencedLock = _fencedLocks.GetOrAdd(key, _ => new FencedLock(objectName, groupId, _cluster, _cpSubsystemSession));
                if (fencedLock.GroupId.Equals(groupId))
                    return fencedLock;

                _fencedLocks.TryRemove(key, out _);
                groupId = await GetGroupIdAsync(groupName).CfAwait();
            }
            */

            while (true)
            {
                if (_fencedLocks.TryGetValue(fullName, out var fencedLock))
                {
                    // if the group ID matches, fine, else we are going to replace the lock
                    if (fencedLock.GroupId.Equals(groupId))
                        return fencedLock;
                    _fencedLocks.TryRemove(fullName, out _);
                }

                // add a new fenced lock - there is a race condition, so another task may add one,
                // and we need to verify that the group ID of the lock we get is correct (in case
                // we don't add but just get the one that was added by the other task) - if it does
                // not match then refresh the group ID and return - we want to be consistent
                fencedLock = _fencedLocks.GetOrAdd(fullName, _ => new FencedLock(fullName, objectName, groupId,
                    _cluster, _cpSubsystemSession, _serializationService));
                if (fencedLock.GroupId.Equals(groupId))
                    return fencedLock;

                groupId = await GetGroupIdAsync(fullName).CfAwait();
            }
        }
        
        public async Task<ICPMap<TKey, TValue>> GetMapAsync<TKey, TValue>([NotNull] string name)
        {
            var (groupName, objectName, fullName) = ParseName(name);
            var groupId = await GetGroupIdAsync(fullName).CfAwait();

            return new CPMap<TKey, TValue>(ServiceNames.CPMap, objectName, _cluster,
                _serializationService, groupId);
        }

        public async Task<ICountDownLatch> GetCountDownLatchAsync(string name)
        {
            var (groupName, objectName, fullName) = ParseName(name);
            var groupId = await GetGroupIdAsync(fullName).CfAwait();

            return new CountDownLatch(objectName, groupId, _cluster, _serializationService);
        }

        // see: ClientRaftProxyFactory.java
        // which also accepts an objectName parameter - but it's only use in toString() = we don't need it

        private async Task<CPGroupId> GetGroupIdAsync(string proxyName)
        {
            var requestMessage = CPGroupCreateCPGroupCodec.EncodeRequest(proxyName);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = CPGroupCreateCPGroupCodec.DecodeResponse(responseMessage).GroupId;
            return response;
        }

        // see: RaftService.java

        internal const string DefaultGroupName = "default";
        internal const string MetaDataGroupName = "METADATA";

        // name should be 'objectName' or 'objectName@groupName'
        public static (string groupName, string objectName, string fullName) ParseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty);

            name = name.Trim();
            var pos = name.IndexOf('@', StringComparison.OrdinalIgnoreCase);

            string groupName;
            if (pos < 0)
            {
                groupName = DefaultGroupName;
            }
            else
            {
                groupName = name[(pos + 1)..].Trim();
                if (groupName.Equals(DefaultGroupName, StringComparison.OrdinalIgnoreCase))
                    groupName = DefaultGroupName;
            }

            if (groupName.Length == 0)
                throw new ArgumentException("CP group name cannot be an empty string.", nameof(name));

            if (groupName.Contains('@', StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("CP group name must be specified at most once.", nameof(name));

            if (groupName.Equals(MetaDataGroupName, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("CP data structures cannot run on the METADATA CP group.");

            var objectName = pos < 0 ? name : name.Substring(0, pos).Trim();

            if (objectName.Length == 0)
                throw new ArgumentException("Object name cannot be empty string.", nameof(name));

            var fullName = objectName + '@' + groupName;

            return (groupName, objectName, fullName);
        }


        public async ValueTask DisposeAsync()
        {
            await _cpSubsystemSession.DisposeAsync().CfAwaitNoThrow();
        }
    }
}
