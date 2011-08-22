package com.hazelcast.elasticmemory;

public interface Storage {
	
	public final static int _1K = 1024;
	public final static int _1M = _1K * _1K;
	
	EntryRef put(int hash, byte[] value);
	
	byte[] get(int hash, EntryRef entry);
	
	void remove(int hash, EntryRef entry);
}
