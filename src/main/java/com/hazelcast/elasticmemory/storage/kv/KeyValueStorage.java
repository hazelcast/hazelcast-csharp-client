package com.hazelcast.elasticmemory.storage.kv;

public interface KeyValueStorage<K> {
	
	void put(K key, byte[] value);
	
	byte[] get(K key);
	
	void remove(K key);
	
	void destroy();
}
