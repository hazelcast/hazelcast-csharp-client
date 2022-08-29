// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading.Tasks;
using Hazelcast.Clustering;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides a base class to CP distributed objects.
    /// </summary>
    internal abstract class CPDistributedObjectBase : ICPDistributedObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CPDistributedObjectBase"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="groupId">The CP group identifier of the object.</param>
        /// <param name="cluster">A cluster.</param>
        protected CPDistributedObjectBase(string serviceName, string name, CPGroupId groupId, Cluster cluster)
        {
            ServiceName = serviceName;
            Name = name;
            CPGroupId = groupId;
            Cluster = cluster;
        }

        /// <inheritdoc />
        public string ServiceName { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public ICPGroupId GroupId => CPGroupId;

        protected CPGroupId CPGroupId { get; }

        /// <inheritdoc />
        public string PartitionKey => null;

        protected Cluster Cluster { get; }

        /// <inheritdoc />
        public abstract ValueTask DestroyAsync();

        /// <summary>
        /// Frees resources used by this instance.
        /// </summary>
        /// <remarks>
        /// Doesn't do anything for now, but some cleanup may be implemented later. <para/>
        /// As such it is recommended to wrap object usage into <code>using</code> statement for better compatibility with future versions.
        /// </remarks>
        public virtual ValueTask DisposeAsync() => default;
    }
}
