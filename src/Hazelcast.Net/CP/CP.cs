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
using Hazelcast.DistributedObjects;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides the <see cref="ICP"/> implementation.
    /// </summary>
    internal class CP : ICP
    {
        private readonly Cluster _cluster;
        private readonly DistributedObjectFactory _distributedOjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="CP"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="distributedOjects">The distributed objects factory.</param>
        public CP(Cluster cluster, DistributedObjectFactory distributedOjects)
        {
            _cluster = cluster;
            _distributedOjects = distributedOjects;
        }

        /// <inheritdoc />
        public async Task<IAtomicLong> GetAtomicLongAsync(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var proxyName = TrimDefaultGroupName(name);
            var objectName = GetProxyObjectName(proxyName);
            var groupId = await GetGroupIdAsync(proxyName, objectName).CfAwait();

            // TODO: GetOrCreateAsync w/ state object to avoid capturing

            return await _distributedOjects.GetOrCreateAsync<IAtomicLong, AtomicLong>(ServiceNames.AtomicLong, name, true,
                (n, f, c, sr, lf)
                    => new AtomicLong(n, groupId, f, c, sr, lf));
        }

        // see: ClientRaftProxyFactory.java

        // FIXME what about the equivalent DESTROY codec?

        private async Task<RaftGroupId> GetGroupIdAsync(string proxyName, string objectName)
        {
            var requestMessage = CPGroupCreateCPGroupCodec.EncodeRequest(proxyName);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = CPGroupCreateCPGroupCodec.DecodeResponse(responseMessage).GroupId;
            return response;
        }

        // see: RaftService.java

        private const string DefaultGroupName = "default";
        private const string MetaDataGroupName = "METADATA";

        public static string TrimDefaultGroupName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            name = name.Trim();
            var i = name.IndexOf('@', StringComparison.OrdinalIgnoreCase);

            if (i == -1)
                return name;

            i++;

            if (name.IndexOf('@', i) >= 0)
                throw new ArgumentException("Custom CP group name must be specified at most once.", nameof(name));

            var groupName = name[i..].Trim();
            return groupName.Equals(DefaultGroupName, StringComparison.OrdinalIgnoreCase) 
                ? name.Substring(0, i - 1) 
                : name;
        }

        public static string GetProxyGroupName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            name = name.Trim();
            var i = name.IndexOf('@', StringComparison.OrdinalIgnoreCase);

            if (i == -1)
                return DefaultGroupName;

            if (i >= name.Length - 1)
                throw new ArgumentException("Custom CP group name cannot be empty string.", nameof(name));

            i++;

            if (name.IndexOf('@', i) >= 0)
                throw new ArgumentException("Custom CP group name must be specified at most once.", nameof(name));

            var groupName = name[(i + 1)..].Trim();

            if (groupName.Length == 0)
                throw new ArgumentException("Custom CP group name cannot be empty string.", nameof(name));

            if (groupName.Equals(MetaDataGroupName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("CCP data structures cannot run on the METADATA CP group.", nameof(name));

            return groupName.Equals(DefaultGroupName, StringComparison.OrdinalIgnoreCase) ? DefaultGroupName : groupName;
        }

        public static string GetProxyObjectName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            name = name.Trim();
            var i = name.IndexOf('@', StringComparison.OrdinalIgnoreCase);
            
            if (i == -1)
                return name;

            if (i >= name.Length - 1) 
                throw new ArgumentException("Object name cannot be empty string.", nameof(name));

            if (name.IndexOf('@', i + 1) >= 0) 
                throw new ArgumentException("Custom CP group name must be specified at most once.", nameof(name));

            var objectName = name.Substring(0, i).Trim();

            if (objectName.Length == 0)
                throw new ArgumentException("Object name cannot be empty string.", nameof(name));

            return objectName;
        }

    }
}