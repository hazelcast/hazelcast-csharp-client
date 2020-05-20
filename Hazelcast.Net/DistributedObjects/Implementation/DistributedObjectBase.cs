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
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Implementation
{
    /// <summary>
    /// Provides a base class to distributed objects.
    /// </summary>
    internal abstract class DistributedObjectBase : IDistributedObject
    {
        private static readonly IPartitioningStrategy PartitioningStrategy = new StringPartitioningStrategy();

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectBase"/> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        protected DistributedObjectBase(string serviceName, string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory) 
        {
            if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(serviceName));
            ServiceName = serviceName;

            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            Name = name;

            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
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
            // FIXME: implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the current thread identifier.
        /// </summary>
        protected long ThreadId => Thread.CurrentThread.ManagedThreadId;

        // FIXME and then below, all common services (and only common services)

        protected ISerializationService SerializationService { get; }

        protected Cluster Cluster { get; }

        protected ILoggerFactory LoggerFactory { get; }

        // TODO: document
        protected virtual IData ToData(object o)
            => SerializationService.ToData(o);

        protected IData ToSafeData(object o1)
        {
            if (o1 == null) throw new ArgumentNullException(nameof(o1));

            var data1 = ToData(o1);

            return data1;
        }

        protected (IData, IData) ToSafeData(object o1, object o2)
        {
            if (o1 == null) throw new ArgumentNullException(nameof(o1));
            if (o2 == null) throw new ArgumentNullException(nameof(o2));

            var data1 = ToData(o1);
            var data2 = ToData(o2);

            return (data1, data2);
        }

        protected (IData, IData, IData) ToSafeData(object o1, object o2, object o3)
        {
            if (o1 == null) throw new ArgumentNullException(nameof(o1));
            if (o2 == null) throw new ArgumentNullException(nameof(o2));
            if (o3 == null) throw new ArgumentNullException(nameof(o3));

            var data1 = ToData(o1);
            var data2 = ToData(o2);
            var data3 = ToData(o3);

            return (data1, data2, data3);
        }

        // TODO: document + isn't 'o' always IData?
        protected virtual TObject ToObject<TObject>(object o)
            => SerializationService.ToObject<TObject>(o);

        public virtual void OnInitialized() {}
    }
}