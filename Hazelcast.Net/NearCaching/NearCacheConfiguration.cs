using System;
using System.Text;
using Hazelcast.Configuration;
using Hazelcast.Core;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Contains the configuration for a Near Cache.
    /// </summary>
    public class NearCacheConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheConfiguration"/> class.
        /// </summary>
        public NearCacheConfiguration()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheConfiguration"/> class.
        /// </summary>
        /// <param name="name">The name of the configuration.</param>
        public NearCacheConfiguration(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of this configuration.
        /// </summary>
        public string Name { get; set; } = "default";

        /// <summary>
        /// Gets or sets the eviction policy.
        /// </summary>
        public EvictionPolicy EvictionPolicy { get; set; } = EvictionPolicy.Lru;

        /// <summary>
        /// Gets or sets the in-memory format.
        /// </summary>
        public InMemoryFormat InMemoryFormat { get; set; } = InMemoryFormat.Binary;

        /// <summary>
        /// Gets or sets the maximum number of seconds an entry can stay in the cache untouched before being evicted.
        /// </summary>
        /// <remarks>
        /// <para>zero means forever.</para>
        /// </remarks>
        public int MaxIdleSeconds { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum size of the cache before entries get evicted.
        /// </summary>
        public int MaxSize { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets or sets the number of seconds entries stay in the cache before being evicted.
        /// </summary>
        public int TimeToLiveSeconds { get; set; } = 0;

        /// <summary>
        /// Whether to invalidate entries when entries in the backing data structure are changed.
        /// </summary>
        /// <remarks>
        /// <para>When true, the cache listens for cluster-wide changes and invalidate entries accordingly.</para>
        /// <para>Changes to the local Hazelcast instance always invalidate the cache immediately.</para>
        /// </remarks>
        public bool InvalidateOnChange { get; set; } = true;

        /// <summary>
        /// TODO: document
        /// </summary>
        public bool SerializeKeys { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder("NearCacheConfig{");
            return text.ToString();
        }
    }
}
