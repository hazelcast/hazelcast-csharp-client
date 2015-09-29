using System;

namespace Hazelcast.Config
{
	public class NearCacheConfigReadOnly : NearCacheConfig
	{
		public NearCacheConfigReadOnly(NearCacheConfig config) : base(config)
		{
		}

		public override NearCacheConfig SetName(string name)
		{
			throw new NotSupportedException("This config is read-only");
		}

		public override NearCacheConfig SetTimeToLiveSeconds(int timeToLiveSeconds)
		{
			throw new NotSupportedException("This config is read-only");
		}

		public override NearCacheConfig SetMaxSize(int maxSize)
		{
			throw new NotSupportedException("This config is read-only");
		}

		public override NearCacheConfig SetEvictionPolicy(string evictionPolicy)
		{
			throw new NotSupportedException("This config is read-only");
		}

		public override NearCacheConfig SetMaxIdleSeconds(int maxIdleSeconds)
		{
			throw new NotSupportedException("This config is read-only");
		}

		public override NearCacheConfig SetInvalidateOnChange(bool invalidateOnChange)
		{
			throw new NotSupportedException("This config is read-only");
		}

		public override NearCacheConfig SetInMemoryFormat(InMemoryFormat inMemoryFormat)
		{
			throw new NotSupportedException("This config is read-only");
		}

		public override NearCacheConfig SetInMemoryFormat(string inMemoryFormat)
		{
			throw new NotSupportedException("This config is read-only");
		}
	}
}
