package com.hazelcast.elasticmemory;

public class EntryRef {
	
	public final int[] indexes;
	public final int length;

	public EntryRef(int[] indexes, int length) {
		this.indexes = indexes;
		this.length = length;
	}
}