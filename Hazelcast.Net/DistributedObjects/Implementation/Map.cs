using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregators;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Data;
using Hazelcast.Data.Map;
using Hazelcast.Exceptions;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Implementation
{
    internal class THIS_IS_TEMP
    {
        // FIXME Task vs ValueTask, document
        // use Task publicly and ValueTask internally? or?
        // skip async/await in places to be faster, but then bad stacktrace?

#if DEBUG // maintain full stack traces
        public async Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
            => await GetDistributedObjectAsync<IMap<TKey,TValue>>(Constants.ServiceNames.Map, name);
#elif RELEASE
        public Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
            => GetDistributedObjectAsync<IMap<TKey,TValue>>(Constants.ServiceNames.Map, name);
#endif

        private async ValueTask<T> GetDistributedObjectAsync<T>(string serviceName, string name)
            where T : IDistributedObject
            => (T) await DOs.GetOrCreateAsync<T>(serviceName, name);

        private DistributedObjects DOs;

    }

    internal class DistributedObjects
    {
        private readonly ConcurrentDictionary<DistributedObjectInfo, ValueTask<IDistributedObject>> _objects
            = new ConcurrentDictionary<DistributedObjectInfo, ValueTask<IDistributedObject>>();

        // why would this need to be concurrent if it's configured once?
        private readonly Dictionary<string, Func<string, Type, DistributedObjectBase>> _factories
            = new Dictionary<string, Func<string, Type, DistributedObjectBase>>();

        private readonly Cluster _cluster;

        public DistributedObjects(Cluster cluster)
        {
            _cluster = cluster;

            _factories[Constants.ServiceNames.Map] = (name, type) =>
            {
                // FIXME aha! we have to create a DistributedObjectBase...
                // but underneath it's a Map<TKey, TValue> which must oooh be a Map<,>
                // and all this code should be in Core not here + no Activator (slow!!)

                bool IsIMapInterface(Type t)
                    => t.IsInterface && t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IMap<,>);

                var interfaceType = IsIMapInterface(type) ? type : type.GetInterfaces().FirstOrDefault(IsIMapInterface);
                if (interfaceType == null)
                    throw new Exception("oops");

                var tKey = interfaceType.GetGenericArguments()[0];
                var tValue = interfaceType.GetGenericArguments()[1];
                var actualType = typeof(Map<,>).MakeGenericType(tKey, tValue);

                return (DistributedObjectBase) Activator.CreateInstance(actualType, Constants.ServiceNames.Map, name);
            };
        }

        public async ValueTask<T> GetOrCreateAsync<T>(string serviceName, string name)
            where T : IDistributedObject
            => (T)await GetOrCreateAsync(typeof(T), serviceName, name, true);

        private async ValueTask<IDistributedObject> GetOrCreateAsync(Type type, string serviceName, string name, bool remote)
        {
            var k = new DistributedObjectInfo(serviceName, name);

            // try to get the object - thanks to the concurrent dictionary there will be only 1 task
            // and if several concurrent requests are made, they will all await that same task
            IDistributedObject o;
            try
            {
                o = await _objects.GetOrAdd(k, x => CreateAsync(type, serviceName, name, remote));
            }
            catch
            {
                _objects.TryRemove(k, out _);
                throw;
            }

            // ReSharper disable once UseMethodIsInstanceOfType
            if (!type.IsAssignableFrom(o.GetType()))
            {
                // if the object that was retrieved is not of the right type, it's a problem
                // preserve the existing object, but throw
                throw new InvalidCastException("A distributed object with the specified service name and name, but "
                                               + "with a different type, has already been created.");
            }

            return o;
        }

        private async ValueTask<IDistributedObject> CreateAsync(Type type, string serviceName, string name, bool remote)
        {
            if (!_factories.TryGetValue(serviceName, out var factory))
                throw new ArgumentException("no factory", nameof(serviceName));

            // TODO was filtered via ExceptionUtil.Rethrow that does weird things?
            var o = factory(name, type);

            // ReSharper disable once UseMethodIsInstanceOfType
            if (!type.IsAssignableFrom(o.GetType()))
            {
                // if the object that was created is not of the right type, it's a problem
                throw new InvalidOperationException("The distributed objects factory created an object with an unexpected type.");
            }

            if (remote) await InitializeRemote(o);
            o.OnInitialized();
            return o;
        }

        private async ValueTask InitializeRemote(IDistributedObject o)
        {
            var requestMessage = ClientCreateProxyCodec.EncodeRequest(o.Name, o.ServiceName);
            _ = await _cluster.SendAsync(requestMessage);
        }
    }

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
        protected DistributedObjectBase(string serviceName, string name)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(serviceName));
            ServiceName = serviceName;

            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            Name = name;
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

        // FIXME and then below, all common services (and only common services)

        public virtual void OnInitialized() {}
    }

    // that's originally implemented by ClientMapProxy
    // which inherits from ClientProxy (DistributedObjectBase)
    // and is managed by a ProxyManager of some sort
    internal class Map<TKey, TValue> : DistributedObjectBase, IMap<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Map{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        public Map(string serviceName, string name)
            : base(serviceName, name)
        { }

        // TODO: reorder these, same as the interface!

        public Task<TValue> AddAsync(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> AddAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(IDictionary<TKey, TValue> entries)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> AddIfMissing(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> AddIfMissing(TKey key, TValue value, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public void AddTransient(TKey key, TValue value, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public TValue Replace(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool Replace(TKey key, TValue expectedValue, TValue newValue)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public bool TryPut(TKey key, TValue value, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> GetAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<TKey, TValue>> GetAllAsync(ICollection<TKey> keys)
        {
            throw new NotImplementedException();
        }

        public Task<IEntryView<TKey, TValue>> GetEntryViewAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet(IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public ISet<TKey> KeySet(IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public ICollection<TValue> Values(IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public bool TryRemoveAsync(TKey key, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> RemoveAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAsync(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task ClearAsync()
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEmpty()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ContainsKeyAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ContainsValueAsync(TValue value)
        {
            throw new NotImplementedException();
        }

        public bool Evict(TKey key)
        {
            throw new NotImplementedException();
        }

        public void EvictAll()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public object ExecuteOnKey(TKey key, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public IDictionary<TKey, object> ExecuteOnKeys(ISet<TKey> keys, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public IDictionary<TKey, object> ExecuteOnEntries(IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public Task<object> SubmitToKey(TKey key, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public void Lock(TKey key)
        {
            throw new NotImplementedException();
        }

        public void Lock(TKey key, TimeSpan leaseTime)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key, TimeSpan timeout, TimeSpan leaseTime)
        {
            throw new NotImplementedException();
        }

        public bool IsLocked(TKey key)
        {
            throw new NotImplementedException();
        }

        public void ForceUnlock(TKey key)
        {
            throw new NotImplementedException();
        }

        public void AddIndex(IndexType type, params string[] attributes)
        {
            throw new NotImplementedException();
        }

        public void AddIndex(IndexConfig indexConfig)
        {
            throw new NotImplementedException();
        }

        public TResult Aggregate<TResult>(IAggregator<TResult> aggregator, IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public ICollection<TResult> Project<TResult>(IProjection projection, IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public void RemoveInterceptor(string id)
        {
            throw new NotImplementedException();
        }

        public bool RemoveEntryListener(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    internal static class Constants
    {
        /// <summary>
        /// Defines service name constants.
        /// </summary>
        internal class ServiceNames
        {
            /// <summary>
            /// Gets the map service name.
            /// </summary>
            public const string Map = "hz:impl:mapService";

            /// <summary>
            /// Gets the topic service name.
            /// </summary>
            public const string Topic = "hz:impl:topicService";

            /// <summary>
            /// Gets the set service name.
            /// </summary>
            public const string Set = "hz:impl:setService";

            /// <summary>
            /// Gets the list service name.
            /// </summary>
            public const string List = "hz:impl:listService";

            /// <summary>
            /// Gets the multi-map service name.
            /// </summary>
            public const string MultiMap = "hz:impl:multiMapService";

            /// <summary>
            /// Gets the PN-counter service name.
            /// </summary>
            public const string PNCounter = "hz:impl:PNCounterService";

            /// <summary>
            /// Gets the cluster service name.
            /// </summary>
            public const string Cluster = "hz:impl:clusterService";

            /// <summary>
            /// Gets the queue service name.
            /// </summary>
            public const string Queue = "hz:impl:queueService";

            /// <summary>
            /// Gets the partition service name.
            /// </summary>
            public const string Partition = "hz:impl:partitionService";

            /// <summary>
            /// Gets the client engine service name.
            /// </summary>
            public const string ClientEngine = "hz:impl:clientEngineService";

            /// <summary>
            /// Gets the ring buffer service name.
            /// </summary>
            public const string Ringbuffer = "hz:impl:ringbufferService";

            /// <summary>
            /// Gets the replicated-map service name.
            /// </summary>
            public const string ReplicatedMap = "hz:impl:replicatedMapService";
        }
    }
}
