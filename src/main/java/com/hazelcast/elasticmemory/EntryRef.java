package com.hazelcast.elasticmemory;

/*
 * Immutable / Thread-safe
 */
 
public class EntryRef {
	
	private final int[] chunks;
	public final int length;

	public EntryRef(int[] indexes, int length) {
		this.chunks = indexes;
		this.length = length;
	}
	
	public boolean isEmpty() {
		return getChunkCount() == 0;
	}
	
	public int getChunkCount() {
		return chunks != null ? chunks.length : 0; 
	}
	
	public int getChunk(int i) {
		return chunks[i];
	}
}