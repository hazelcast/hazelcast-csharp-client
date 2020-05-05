using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    /// <summary>
    /// Implements <see cref="IMap{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal partial class Map<TKey, TValue> : DistributedObjectBase, IMap<TKey, TValue>
    {
        private readonly ISequence<long> _lockReferenceIdSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        public Map(string serviceName, string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence)
            : base(serviceName, name, cluster, serializationService)
        {
            _lockReferenceIdSequence = lockReferenceIdSequence;
        }

        // TODO no timeout management or CancellationToken anywhere?!
    }
}
