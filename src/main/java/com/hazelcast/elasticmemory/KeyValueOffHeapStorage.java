package com.hazelcast.elasticmemory;

import static com.hazelcast.elasticmemory.Util.*;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.locks.ReentrantLock;

public class KeyValueOffHeapStorage<K> extends OffHeapStorageSupport implements KeyValueStorage<K> {
	
	private final StorageSegment[] segments ;
	
	public KeyValueOffHeapStorage(int totalSizeInMb, int chunkSizeInKb) {
		this(totalSizeInMb, Math.max(MIN_SEGMENT_COUNT, divideAndCeil(totalSizeInMb, MAX_SEGMENT_SIZE_IN_MB)), chunkSizeInKb);
	}
	
	public KeyValueOffHeapStorage(int totalSizeInMb, int segmentCount, int chunkSizeInKb) {
		super(totalSizeInMb, segmentCount, chunkSizeInKb);
		
		this.segments = new StorageSegment[segmentCount];
		for (int i = 0; i < segmentCount; i++) {
			segments[i] = new StorageSegment<K>(segmentSizeInMb, chunkSizeInKb);
		}
	}
	
	public void put(K key, byte[] value) {
		getSegment(key).put(key, value);
	}

	public byte[] get(K key) {
		return getSegment(key).get(key);
	}
	
	public void remove(K key) {
		getSegment(key).remove(key);
	}

	private StorageSegment<K> getSegment(K key) {
		int hash = key.hashCode();
		return segments[(hash == Integer.MIN_VALUE) ? 0 : Math.abs(hash) % segmentCount];
	}
	
	private class StorageSegment<K> extends ReentrantLock {
		
		private final ByteBufferStorage storage;
		private final Map<K, EntryRef> space;

		StorageSegment(int totalSizeInMb, int chunkSizeInKb) {
			super();
			space = new HashMap<K, EntryRef>(divideAndCeil(totalSizeInMb * Storage._1K, chunkSizeInKb * 2));
			storage = new ByteBufferStorage(totalSizeInMb, chunkSizeInKb);
		}

		public void put(final K key, final byte[] value) {
			lock();
			try {
				remove0(key);
				EntryRef ref = storage.put(value);
				space.put(key, ref);
			} finally {
				unlock();
			}
		}

		public byte[] get(final K key) {
			lock();
			try {
				final EntryRef ref = space.get(key);
				if (ref == null || ref.indexes == null || ref.indexes.length == 0) {
					space.remove(key);
					return null;
				}
				return storage.get(ref);
			} finally {
				unlock();
			}
		}
		
		public void remove(final K key) {
			lock();
			try {
				remove0(key);
			} finally {
				unlock();
			}
		}
		
		private void remove0(final K key) {
			final EntryRef ref = space.remove(key);
			storage.remove(ref);
		}
	}
}
