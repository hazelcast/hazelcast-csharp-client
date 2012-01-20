using System;

namespace Hazelcast.Client
{
	public class ClientMapEntry<K,V>: Hazelcast.Core.MapEntry<K,V>
	{
		private CMapEntry mapEntry;
    	private K key;
    	private MapClientProxy<K, V> proxy;

	    public ClientMapEntry(CMapEntry mapEntry, K key, MapClientProxy<K, V> proxy) {
	        this.mapEntry = mapEntry;
	        this.key = key;
	        this.proxy = proxy;
	    }
	
	    public long getCost() {
	        return mapEntry.getCost();
	    }
	
	    public long getCreationTime() {
	        return mapEntry.getCreationTime();
	    }
	
	    public long getExpirationTime() {
	        return mapEntry.getExpirationTime();
	    }
	
	    public int getHits() {
	        return mapEntry.getHits();
	    }
	
	    public long getLastAccessTime() {
	        return mapEntry.getLastAccessTime();
	    }
	
	    public long getLastStoredTime() {
	        return mapEntry.getLastStoredTime();
	    }
	
	    public long getLastUpdateTime() {
	        return mapEntry.getLastUpdateTime();
	    }
	
	    public long getVersion() {
	        return mapEntry.getVersion();
	    }
	
	    public bool isValid() {
	        return mapEntry.isValid();
	    }
	
	    public K getKey() {
	        return key;
	    }
	
	    public V getValue() {
	        return proxy.get(key);
	    }
	
	    public V setValue(V value) {
	        return proxy.put(key, value);
	    }
	
	   
	    public override String ToString() {
	        System.Text.StringBuilder sb = new System.Text.StringBuilder();
	        sb.Append("MapEntry");
	        sb.Append("{key=").Append(key);
	        sb.Append(", valid=").Append(isValid());
	        sb.Append(", hits=").Append(getHits());
	        sb.Append(", version=").Append(getVersion());
	        sb.Append(", creationTime=").Append(getCreationTime());
	        sb.Append(", lastUpdateTime=").Append(getLastUpdateTime());
	        sb.Append(", lastAccessTime=").Append(getLastAccessTime());
	        sb.Append(", expirationTime=").Append(getExpirationTime());
	        sb.Append(", cost=").Append(getCost());
	        sb.Append('}');
	        return sb.ToString();
	    }
	}
}

