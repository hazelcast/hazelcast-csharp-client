package com.hazelcast.elasticmemory.storage;

import static com.hazelcast.elasticmemory.util.MathUtil.*;

import java.nio.ByteBuffer;
import java.util.logging.Level;

import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;

public class BufferSegment {
	
	private static final ILogger logger = Logger.getLogger(BufferSegment.class.getName());
	
	public final static int _1K = 1024;
	public final static int _1M = _1K * _1K;

	private static int ID = 0;
	private static synchronized int nextId() {
		return ID++;
	}
	
	private final int totalSize;
	private final int chunkSize;
	private final int chunkCount;
	private final AddressQueue chunks;
	private final ByteBuffer buffer;

	public BufferSegment(int totalSizeInMb, int chunkSizeInKb) {
		super();
		this.totalSize = totalSizeInMb * _1M;
		this.chunkSize = chunkSizeInKb * _1K;
		
		assertTrue((totalSize % chunkSize == 0), "Segment size[" + totalSizeInMb 
					+ " MB] must be multitude of chunk size["+ chunkSizeInKb + " KB]!");
		
		int index = nextId();
		this.chunkCount = totalSize / chunkSize;
		logger.log(Level.FINEST, "BufferSegment[" + index + "] starting with chunkCount=" + chunkCount);

		chunks = new AddressQueue(chunkCount);
		buffer = ByteBuffer.allocateDirect(totalSize);
		for (int i = 0; i < chunkCount; i++) {
			chunks.offer(i);
		}
		logger.log(Level.INFO, "BufferSegment[" + index + "] started!");
	}

	public EntryRef put(final byte[] value) {
		if (value.length == 0) {
			return null;
		}

		final int count = divideByAndCeil(value.length, chunkSize);
		final int[] indexes = chunks.poll(count);
		final EntryRef ref = new EntryRef(indexes, value.length);

		int offset = 0;
		for (int i = 0; i < count; i++) {
			buffer.position(indexes[i] * chunkSize);
			int len = Math.min(chunkSize, (ref.length - offset));
			buffer.put(value, offset, len);
			offset += len;
		}
		return ref;
	}

	public byte[] get(final EntryRef ref) {
		if (!isEntryRefValid(ref)) {
			return null;
		}

		final byte[] value = new byte[ref.length];
		final int chunkCount = ref.getChunkCount();
		int offset = 0;
		for (int i = 0; i < chunkCount; i++) {
			buffer.position(ref.getChunk(i) * chunkSize);
			int len = Math.min(chunkSize, (ref.length - offset));
			buffer.get(value, offset, len);
			offset += len;
		}
		return value;
	}

	public void remove(final EntryRef ref) {
		if (!isEntryRefValid(ref)) {
			return;
		}

		final int chunkCount = ref.getChunkCount();
		for (int i = 0; i < chunkCount; i++) {
			assertTrue(chunks.offer(ref.getChunk(i)), "Could not offer released indexes! Error in queue...");
		}
		ref.invalidate();
	}

	private boolean isEntryRefValid(EntryRef ref) {
		return ref != null && !ref.isEmpty() && ref.isValid();
	}
	
	private static void assertTrue(boolean condition, String message) {
		if(!condition) {
			throw new AssertionError(message);
		}
	}

	private class AddressQueue {
		final static int NULL_VALUE = -1;
		final int maxSize;
		final int[] array;
		int add = 0;
		int remove = 0;
		int size = 0;

		public AddressQueue(int maxSize) {
			this.maxSize = maxSize;
			array = new int[maxSize];
		}

		public boolean offer(int value) {
			if (size == maxSize) {
				return false;
			}
			array[add++] = value;
			size++;
			if (add == maxSize) {
				add = 0;
			}
			return true;
		}

		public int poll() {
			if (size == 0) {
				return NULL_VALUE;
			}
			final int value = array[remove];
			array[remove++] = NULL_VALUE;
			size--;
			if (remove == maxSize) {
				remove = 0;
			}
			return value;
		}
		
		public int[] poll(final int count) {
			if(count > size) {
				throwOutOfMemoryError("Segment has " + size + " available chunks. " +
						"Data requires " + count + " chunks. Segment is full!");
			}
			
			final int[] result = new int[count];
			for (int i = 0; i < count; i++) {
				result[i] = poll();
			}
			return result;
		}
	}
	
	static void throwOutOfMemoryError(String error) {
		throw new OffHeapOutOfMemoryError(error);
	}
}
