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
using System.Threading;
using Hazelcast.Clustering;
using Hazelcast.Exceptions;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation
{
    /// <summary>
    /// Provides a base class to distributed objects.
    /// </summary>
    internal abstract class DistributedObjectBase : IDistributedObject
    {
        private static readonly IPartitioningStrategy PartitioningStrategy = new StringPartitioningStrategy();

        private readonly ISerializationService _serializationService;

        // TODO consider injecting a "light" cluster not the whole class?

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectBase"/> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        protected DistributedObjectBase(string serviceName, string name, Cluster cluster, ISerializationService serializationService) 
        {
            if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(serviceName));
            ServiceName = serviceName;

            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            Name = name;

            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
        }

        /// <inheritdoc />
        public string ServiceName { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string PartitionKey => (string) PartitioningStrategy.GetPartitionKey(Name); // FIXME doh?

        /// <inheritdoc />
        public void Destroy()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the current thread identifier.
        /// </summary>
        protected long ThreadId => Thread.CurrentThread.ManagedThreadId;

        // FIXME and then below, all common services (and only common services)

        protected Cluster Cluster { get; }

        // TODO: document
        protected virtual IData ToData(object o)
            => _serializationService.ToData(o);

        // TODO: document + isn't 'o' always IData?
        protected virtual TObject ToObject<TObject>(object o)
            => _serializationService.ToObject<TObject>(o);

        public virtual void OnInitialized() {}
    }
}