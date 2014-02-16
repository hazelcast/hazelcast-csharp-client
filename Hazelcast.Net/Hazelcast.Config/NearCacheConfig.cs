using System;
using System.Text;

namespace Hazelcast.Config
{
    public class NearCacheConfig
    {
        public const int DefaultTtlSeconds = 0;

        public const int DefaultMaxIdleSeconds = 0;

        public const int DefaultMaxSize = int.MaxValue;

        public const string DefaultEvictionPolicy = "Lru";

        public static readonly InMemoryFormat DefaultMemoryFormat = InMemoryFormat.Binary;

        private int timeToLiveSeconds = DefaultTtlSeconds;

        private int maxSize = DefaultMaxSize;

        private string evictionPolicy = DefaultEvictionPolicy;

        private int maxIdleSeconds = DefaultMaxIdleSeconds;

        private bool invalidateOnChange = true;

        private InMemoryFormat inMemoryFormat = DefaultMemoryFormat;

        private string name = "default";

        private NearCacheConfigReadOnly readOnly;

        private bool cacheLocalEntries = false;

        public NearCacheConfig(int timeToLiveSeconds, int maxSize, string evictionPolicy, int maxIdleSeconds, bool invalidateOnChange, InMemoryFormat inMemoryFormat)
        {
            this.timeToLiveSeconds = timeToLiveSeconds;
            this.maxSize = maxSize;
            this.evictionPolicy = evictionPolicy;
            this.maxIdleSeconds = maxIdleSeconds;
            this.invalidateOnChange = invalidateOnChange;
            this.inMemoryFormat = inMemoryFormat;
        }

        public NearCacheConfig(Hazelcast.Config.NearCacheConfig config)
        {
            name = config.GetName();
            evictionPolicy = config.GetEvictionPolicy();
            inMemoryFormat = config.GetInMemoryFormat();
            invalidateOnChange = config.IsInvalidateOnChange();
            maxIdleSeconds = config.GetMaxIdleSeconds();
            maxSize = config.GetMaxSize();
            timeToLiveSeconds = config.GetTimeToLiveSeconds();
            cacheLocalEntries = config.IsCacheLocalEntries();
        }

        public virtual NearCacheConfigReadOnly GetAsReadOnly()
        {
            if (readOnly == null)
            {
                readOnly = new NearCacheConfigReadOnly(this);
            }
            return readOnly;
        }

        public NearCacheConfig()
        {
        }

        public virtual string GetName()
        {
            return name;
        }

        public virtual void SetName(string name)
        {
            this.name = name;
        }

        public virtual int GetTimeToLiveSeconds()
        {
            return timeToLiveSeconds;
        }

        public virtual NearCacheConfig SetTimeToLiveSeconds(int timeToLiveSeconds)
        {
            this.timeToLiveSeconds = timeToLiveSeconds;
            return this;
        }

        public virtual int GetMaxSize()
        {
            return maxSize;
        }

        public virtual NearCacheConfig SetMaxSize(int maxSize)
        {
            this.maxSize = maxSize;
            return this;
        }

        public virtual string GetEvictionPolicy()
        {
            return evictionPolicy;
        }

        public virtual NearCacheConfig SetEvictionPolicy(string evictionPolicy)
        {
            this.evictionPolicy = evictionPolicy;
            return this;
        }

        public virtual int GetMaxIdleSeconds()
        {
            return maxIdleSeconds;
        }

        public virtual NearCacheConfig SetMaxIdleSeconds(int maxIdleSeconds)
        {
            this.maxIdleSeconds = maxIdleSeconds;
            return this;
        }

        public virtual bool IsInvalidateOnChange()
        {
            return invalidateOnChange;
        }

        public virtual NearCacheConfig SetInvalidateOnChange(bool invalidateOnChange)
        {
            this.invalidateOnChange = invalidateOnChange;
            return this;
        }

        public virtual InMemoryFormat GetInMemoryFormat()
        {
            return inMemoryFormat;
        }

        public virtual NearCacheConfig SetInMemoryFormat(InMemoryFormat inMemoryFormat)
        {
            this.inMemoryFormat = inMemoryFormat;
            return this;
        }

        public virtual bool IsCacheLocalEntries()
        {
            return cacheLocalEntries;
        }

        public virtual Hazelcast.Config.NearCacheConfig SetCacheLocalEntries(bool cacheLocalEntries)
        {
            this.cacheLocalEntries = cacheLocalEntries;
            return this;
        }

        // this setter is for reflection based configuration building
        public virtual NearCacheConfig SetInMemoryFormat(string inMemoryFormat)
        {
            Enum.TryParse(inMemoryFormat, true, out this.inMemoryFormat);
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("NearCacheConfig{");
            sb.Append("timeToLiveSeconds=").Append(timeToLiveSeconds);
            sb.Append(", maxSize=").Append(maxSize);
            sb.Append(", evictionPolicy='").Append(evictionPolicy).Append('\'');
            sb.Append(", maxIdleSeconds=").Append(maxIdleSeconds);
            sb.Append(", invalidateOnChange=").Append(invalidateOnChange);
            sb.Append(", inMemoryFormat=").Append(inMemoryFormat);
            sb.Append(", cacheLocalEntries=").Append(cacheLocalEntries);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
