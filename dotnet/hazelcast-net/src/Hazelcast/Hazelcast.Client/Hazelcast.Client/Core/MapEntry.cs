using System;

namespace Hazelcast.Core
{
	public interface MapEntry<K,V>
	{
		long getCost();
		
		long getCreationTime();
		
		long getExpirationTime();
		
		int getHits();
		
		long getLastAccessTime();
		
		long getLastStoredTime();
		
		long getLastUpdateTime();
		
		long getVersion();
		
		bool isValid();
		
		K getKey();
		
		V getValue();
		
		V setValue(V key);
	}
}

