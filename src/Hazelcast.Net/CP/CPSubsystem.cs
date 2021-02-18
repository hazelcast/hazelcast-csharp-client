// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides the <see cref="ICPSubsystem"/> implementation.
    /// </summary>
    internal class CPSubsystem : ICPSubsystem
    {
        private readonly Cluster _cluster;

        /// <summary>
        /// Initializes a new instance of the <see cref="CPSubsystem"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public CPSubsystem(Cluster cluster)
        {
            _cluster = cluster;
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
        // The are destroyed via ClientProxy.destroy, which is getContext().getProxyManager().destroyProxy(this),
        // which means they are destroyed by ProxyManager aka DistributedObjectFactory, which would try to
        // remove them from cache (always missing) and end up doing proxy.destroyLocally() which eventually
        // calls into the object's onDestroy() method.
        //
        // But... this is convoluted? For now, our objects inherit from CPObjectBase which is simpler than
        // DistributedObjectBase, they do not hit DistributedObjectFactory at all, and implement their
        // own destroy method.

        /// <inheritdoc />
        public async Task<IAtomicLong> GetAtomicLongAsync(string name)
        {
            var (groupName, objectName) = ParseName(name);
            var groupId = await GetGroupIdAsync(groupName, objectName).CfAwait();

            return new AtomicLong(objectName, groupId, _cluster);
        }

        // see: ClientRaftProxyFactory.java

        private async Task<CPGroupId> GetGroupIdAsync(string proxyName, string objectName)
        {
            var requestMessage = CPGroupCreateCPGroupCodec.EncodeRequest(proxyName);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = CPGroupCreateCPGroupCodec.DecodeResponse(responseMessage).GroupId;
            return response;
        }

        // see: RaftService.java

        internal const string DefaultGroupName = "default";
        internal const string MetaDataGroupName = "METADATA";

        public static (string groupName, string objectName) ParseName(string name)
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
                groupName = name.Substring(pos + 1).Trim();
                if (groupName.Equals(DefaultGroupName, StringComparison.OrdinalIgnoreCase))
                    groupName = DefaultGroupName;
            }

            if (groupName.Length == 0)
                throw new ArgumentException("CP group name cannot be an empty string.", nameof(name));

            if (groupName.Contains("@"))
                throw new ArgumentException("CP group name must be specified at most once.", nameof(name));

            if (groupName.Equals(MetaDataGroupName, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("CP data structures cannot run on the METADATA CP group.");

            var objectName = pos < 0 ? name : name.Substring(0, pos).Trim();

            if (objectName.Length == 0)
                throw new ArgumentException("Object name cannot be empty string.", nameof(name));

            return (groupName, objectName);
        }
    }
}