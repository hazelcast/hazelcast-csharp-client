using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    /// <summary>
    /// Provides constants for the map type.
    /// </summary>
    internal class Map
    {
        /// <summary>
        /// Gets the service name.
        /// </summary>
        public const string ServiceName = "hz:impl:mapService";
    }

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
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        /// <param name="logggerFactory">A logger factory.</param>
        public Map(string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence, ILoggerFactory logggerFactory)
            : base(Map.ServiceName, name, cluster, serializationService, logggerFactory)
        {
            _lockReferenceIdSequence = lockReferenceIdSequence;
        }

        // TODO no timeout management or CancellationToken anywhere?!
    }
}
