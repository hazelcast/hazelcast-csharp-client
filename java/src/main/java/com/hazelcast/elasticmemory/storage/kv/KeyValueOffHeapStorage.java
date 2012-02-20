package com.hazelcast.elasticmemory.storage.kv;

/*public class KeyValueOffHeapStorage<K> extends OffHeapStorageSupport implements KeyValueStorage<K> {
	
	private final StorageSegment[] segments ;
	
	public KeyValueOffHeapStorage(int totalSizeInMb, int chunkSizeInKb) {
		this(totalSizeInMb, divideByAndCeil(totalSizeInMb, MAX_SEGMENT_SIZE_IN_MB), chunkSizeInKb);
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
	
	public void destroy() {
		destroy(segments);
	}
	
	private class StorageSegment<K> extends ReentrantLock implements Closeable {
		
		private BufferSegment buffer;
		private final Map<K, EntryRef> space;

		StorageSegment(int totalSizeInMb, int chunkSizeInKb) {
			super();
			space = new HashMap<K, EntryRef>(divideByAndCeil(totalSizeInMb * Storage._1K, chunkSizeInKb * 2));
			buffer = new BufferSegment(totalSizeInMb, chunkSizeInKb);
		}

		void put(final K key, final byte[] value) {
			lock();
			try {
				remove0(key);
				EntryRef ref = buffer.put(value);
				space.put(key, ref);
			} finally {
				unlock();
			}
		}

        // TODO: update for lock free implementation!
		byte[] get(final K key) {
			lock();
			try {
				final EntryRef ref = space.get(key);
				if (ref == null || ref.isEmpty()) {
					space.remove(key);
					return null;
				}
				OffHeapData value = buffer.get(ref); // under-lock, can not be invalid!
                return value != null ? value.data : null;
			} finally {
				unlock();
			}
		}
		
		void remove(final K key) {
			lock();
			try {
				remove0(key);
			} finally {
				unlock();
			}
		}
		
		private void remove0(final K key) {
			final EntryRef ref = space.remove(key);
			buffer.remove(ref);
		}
		
		public void close() {
			if(buffer != null) {
				buffer.destroy();
				buffer = null;
			}
			space.clear();
		}
	}
}*/
