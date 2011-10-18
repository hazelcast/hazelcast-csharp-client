package com.hazelcast.elasticmemory.storage;

/*
 * Thread-safe
 */
 
public class EntryRef {
	
	public final int length;
	private final int[] chunks;
	private /*volatile*/ boolean valid = true;

	EntryRef(int[] indexes, int length) {
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

	boolean isValid() {
		return valid;
	}
	
	void invalidate() {
		valid = false;
	}
	
	protected Object clone() throws CloneNotSupportedException {
		throw new CloneNotSupportedException();
	}
}