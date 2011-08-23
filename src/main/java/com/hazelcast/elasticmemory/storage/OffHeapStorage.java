package com.hazelcast.elasticmemory.storage;

import static com.hazelcast.elasticmemory.Util.*;

import java.util.concurrent.locks.ReentrantLock;

import com.hazelcast.elasticmemory.EntryRef;

public class OffHeapStorage extends OffHeapStorageSupport implements Storage {
	
	private final StorageSegment[] segments ;
	
	public OffHeapStorage(int totalSizeInMb, int chunkSizeInKb) {
		this(totalSizeInMb, Math.max(MIN_SEGMENT_COUNT, divideAndCeil(totalSizeInMb, MAX_SEGMENT_SIZE_IN_MB)), chunkSizeInKb);
	}
	
	public OffHeapStorage(int totalSizeInMb, int segmentCount, int chunkSizeInKb) {
		super(totalSizeInMb, segmentCount, chunkSizeInKb);
		
		this.segments = new StorageSegment[segmentCount];
		for (int i = 0; i < segmentCount; i++) {
			segments[i] = new StorageSegment(segmentSizeInMb, chunkSizeInKb);
		}
	}
	
	private StorageSegment getSegment(int hash) {
		return segments[(hash == Integer.MIN_VALUE) ? 0 : Math.abs(hash) % segmentCount];
	}

	public EntryRef put(int hash, byte[] value) {
		return getSegment(hash).put(value);
	}

	public byte[] get(int hash, EntryRef entry) {
		return getSegment(hash).get(entry);
	}

	public void remove(int hash, EntryRef entry) {
		getSegment(hash).remove(entry);
	}
	
	private class StorageSegment extends ReentrantLock {
		private final ByteBufferStorage storage;

		StorageSegment(int totalSizeInMb, int chunkSizeInKb) {
			super();
			storage = new ByteBufferStorage(totalSizeInMb, chunkSizeInKb);
		}

		public EntryRef put(final byte[] value) {
			lock();
			try {
				return storage.put(value);
			} finally {
				unlock();
			}
		}

		public byte[] get(final EntryRef entry) {
			lock();
			try {
				return storage.get(entry);
			} finally {
				unlock();
			}
		}
		
		public void remove(final EntryRef entry) {
			lock();
			try {
				storage.remove(entry);
			} finally {
				unlock();
			}
		}
	}
}
