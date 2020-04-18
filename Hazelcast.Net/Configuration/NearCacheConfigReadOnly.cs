using System;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Readonly version of <see cref="NearCacheConfig"/>
    /// </summary>
    public class NearCacheConfigReadOnly : NearCacheConfig
    {

        public NearCacheConfigReadOnly(NearCacheConfig config) : base(config)
        {
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetEvictionPolicy(string evictionPolicy)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetInMemoryFormat(InMemoryFormat inMemoryFormat)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetInMemoryFormat(string inMemoryFormat)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetInvalidateOnChange(bool invalidateOnChange)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetMaxIdleSeconds(int maxIdleSeconds)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetMaxSize(int maxSize)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetName(string name)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetTimeToLiveSeconds(int timeToLiveSeconds)
        {
            throw new NotSupportedException("This config is read-only");
        }
    }
}
