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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Provides a base class to distributed objects.
    /// </summary>
    internal abstract class DistributedObjectBase : IDistributedObject
    {
        private static readonly IPartitioningStrategy PartitioningStrategy = new StringPartitioningStrategy();

        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Action<DistributedObjectBase> _onDispose;
        private string _partitionKey;
        private IData _partitionKeyData;

        private volatile int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectBase"/> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
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

        /// <summary>
        /// Gets or sets an action that will be executed when the object disposes.
        /// </summary>
        public Action<DistributedObjectBase> OnDispose
        {
            get => _onDispose;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onDispose = value;
            }
        }

        /// <inheritdoc />
        public string ServiceName { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        // FIXME: PartitionKey always uses simple String strategy = ?! (see Java?)
        public string PartitionKey => _partitionKey ??= (string) PartitioningStrategy.GetPartitionKey(Name);

        // FIXME document
        public IData PartitionKeyData => _partitionKeyData ??= ToData(PartitionKey);

        /// <summary>
        /// Gets the current context identifier.
        /// </summary>
        /// <remarks>
        /// Hazelcast APIs call this the thread identified and maintain locks "per threads",
        /// so we are keeping the name here internally, but in reality this is not a thread
        /// identifier anymore - it is attached to the async context so it can flow with
        /// async operations.
        /// </remarks>
        protected static long ContextId => AsyncContext.CurrentContext.Id;

        /// <summary>
        /// Gets the serialization service.
        /// </summary>
        protected ISerializationService SerializationService { get; }

        /// <summary>
        /// Gets the cluster.
        /// </summary>
        protected Cluster Cluster { get; }

        /// <summary>
        /// Gets the default operation timeout in milliseconds.
        /// </summary>
        protected int DefaultOperationTimeoutMilliseconds => Cluster.DefaultOperationTimeoutMilliseconds;

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Serializes an object to <see cref="IData"/>.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns>The <see cref="IData"/> serialized object.</returns>
        protected virtual IData ToData(object o)
            => SerializationService.ToData(o);

        /// <summary>
        /// Serializes a non-null object to <see cref="IData"/>.
        /// </summary>
        /// <param name="o1">The object.</param>
        /// <returns>The <see cref="IData"/> serialized object.</returns>
        /// <exception cref="ArgumentNullException">Occurs when the object is null.</exception>
        protected IData ToSafeData(object o1)
        {
            if (o1 == null) throw new ArgumentNullException(nameof(o1));

            var data1 = ToData(o1);

            return data1;
        }

        /// <summary>
        /// Serializes non-null objects to <see cref="IData"/>.
        /// </summary>
        /// <typeparam name="T">The type of the objects.</typeparam>
        /// <param name="collection">The collection of objects.</param>
        /// <returns>The collection of <see cref="IData"/> serialized objects.</returns>
        /// <exception cref="ArgumentNullException">Occurs when the collection, or an object in the collection, is null.</exception>
        protected ICollection<IData> ToSafeData<T>(ICollection<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var data = new List<IData>(collection.Count);
            foreach (var item in collection) data.Add(ToData(item));
            return data;
        }

        /// <summary>
        /// Serializes non-null objects to <see cref="IData"/>.
        /// </summary>
        /// <param name="o1">The first object.</param>
        /// <param name="o2">The second objects.</param>
        /// <returns>The <see cref="IData"/> serialized objects.</returns>
        /// <exception cref="ArgumentNullException">Occurs when any of the objects is null.</exception>
        protected (IData, IData) ToSafeData(object o1, object o2)
        {
            if (o1 == null) throw new ArgumentNullException(nameof(o1));
            if (o2 == null) throw new ArgumentNullException(nameof(o2));

            var data1 = ToData(o1);
            var data2 = ToData(o2);

            return (data1, data2);
        }

        /// <summary>
        /// Serializes non-null objects to <see cref="IData"/>.
        /// </summary>
        /// <param name="o1">The first object.</param>
        /// <param name="o2">The second objects.</param>
        /// <param name="o3">The third objects.</param>
        /// <returns>The <see cref="IData"/> serialized objects.</returns>
        /// <exception cref="ArgumentNullException">Occurs when any of the objects is null.</exception>
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

        /// <summary>
        /// Deserializes an <see cref="IData"/> serialized instance into an object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="data">The <see cref="IData"/> serialized instance.</param>
        /// <returns>The deserialized object.</returns>
        protected virtual TObject ToObject<TObject>(IData data)
            => SerializationService.ToObject<TObject>(data);

        /// <summary>
        /// Creates a lazy argument.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument.</typeparam>
        /// <param name="source">The source value.</param>
        /// <returns>The lazy argument.</returns>
        protected Lazy<TArg> LazyArg<TArg>(IData source)
            => source == null
                ? null
                : new Lazy<TArg>(() => ToObject<TArg>(source));

        /// <summary>
        /// Represents subscription state data.
        /// </summary>
        /// <typeparam name="TEventHandlers">The type of the event handlers.</typeparam>
        protected class SubscriptionState<TEventHandlers>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SubscriptionState{TEventHandlers}"/> class.
            /// </summary>
            /// <param name="name">The unique name of the distributed object.</param>
            /// <param name="handlers">The event handlers.</param>
            public SubscriptionState(string name, TEventHandlers handlers)
            {
                Name = name;
                Handlers = handlers;
            }

            /// <summary>
            /// Gets the unique name of the distributed object.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the event handlers.
            /// </summary>
            public TEventHandlers Handlers { get; }
        }

        /// <summary>
        /// Casts a subscription state, or throw.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="state">The state object.</param>
        /// <returns>The state object.</returns>
        protected static T ToSafeState<T>(object state)
        {
            return state switch
            {
                null => default,
                T sstate => sstate,
                _ => throw new InvalidCastException("Invalid subscription state type.")
            };
        }

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <param name="timeout">A timeout.</param>
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task UnsubscribeAsync(Guid subscriptionId, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(UnsubscribeAsync, subscriptionId, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task UnsubscribeAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var task = Cluster.RemoveSubscriptionAsync(subscriptionId, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        public virtual void OnInitialized()
        {
            _readonlyProperties = true;
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return default;

            _onDispose(this);

            return DisposeAsyncInternal();
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        protected virtual ValueTask DisposeAsyncInternal() => default;
    }
}
