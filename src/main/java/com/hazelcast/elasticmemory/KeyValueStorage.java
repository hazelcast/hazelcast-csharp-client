package com.hazelcast.elasticmemory;

public interface KeyValueStorage<K> {
	
	void put(K key, byte[] value);
	
	byte[] get(K key);
	
	void remove(K key);
	
}
