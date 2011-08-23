package com.hazelcast.elasticmemory.storage;

import static com.hazelcast.elasticmemory.Util.*;
import java.nio.ByteBuffer;

import com.hazelcast.elasticmemory.EntryRef;

public class ByteBufferStorage {
	
	public final static int _1K = 1024;
	public final static int _1M = _1K * _1K;

	private static int ID = 0;
	private static synchronized int nextId() {
		return ID++;
	}
	
	private final int totalSize;
	private final int chunkSize;
	private final int chunkCount;
	private final IntQueue chunks;
	private final ByteBuffer buffer;

	public ByteBufferStorage(int totalSizeInMb, int chunkSizeInKb) {
		super();
		this.totalSize = totalSizeInMb * _1M;
		this.chunkSize = chunkSizeInKb * _1K;
		
		assertTrue((totalSize % chunkSize == 0), "Segment size[" + totalSizeInMb 
					+ " MB] must be multitude of chunk size["+ chunkSizeInKb + " KB]!");
		
		int index = nextId();
		this.chunkCount = totalSize / chunkSize;
		System.out.println("BufferSegment[" + index + "] starting with chunkCount=" + chunkCount);

		chunks = new IntQueue(chunkCount);
		buffer = ByteBuffer.allocateDirect(totalSize);
		for (int i = 0; i < chunkCount; i++) {
			chunks.offer(i);
		}
		System.out.println("BufferSegment[" + index + "] started!");
	}

	public EntryRef put(final byte[] value) {
		final int length = value.length;
		if (length == 0) {
			return null;
		}

		final int count = divideAndCeil(length, chunkSize);
		final int[] indexes = new int[count];
		final EntryRef ref = new EntryRef(indexes, length);

		int offset = 0;
		for (int i = 0; i < count; i++) {
			int index = chunks.poll();
			if (index == IntQueue.NULL_VALUE) {
				throwOutOfMemoryError("Segment is full!!!");
			}
			buffer.position(index * chunkSize);
			int l = Math.min(chunkSize, (length - offset));
			buffer.put(value, offset, l);
			indexes[i] = index;
			offset += l;
		}
		return ref;
	}

	public byte[] get(final EntryRef ref) {
		if (ref == null || ref.indexes == null || ref.indexes.length == 0) {
			return null;
		}

		final int[] indexes = ref.indexes;
		final int length = ref.length;
		final byte[] value = new byte[length];

		final int chunks = indexes.length;
		int offset = 0;
		for (int i = 0; i < chunks; i++) {
			buffer.position(indexes[i] * chunkSize);
			int len = Math.min(chunkSize, (length - offset));
			buffer.get(value, offset, len);
			offset += len;
		}
		return value;
	}

	public void remove(final EntryRef ref) {
		if (ref == null || ref.indexes == null || ref.indexes.length == 0) {
			return;
		}

		final int[] indexes = ref.indexes;
		for (int i = 0; i < indexes.length; i++) {
			assertTrue(chunks.offer(indexes[i]), "Could not offer released indexes! Error in queue...");
		}
	}

	private static void assertTrue(boolean condition, String message) {
		if(!condition) {
			throw new AssertionError(message);
		}
	}

	private class IntQueue {
		final static int NULL_VALUE = -1;
		final int maxSize;
		final int[] array;
		int add = 0;
		int remove = 0;
		int size = 0;

		public IntQueue(int maxSize) {
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
	}
	
	abstract static class OffHeapError extends Error {
		public OffHeapError() {
			super();
		}
		public OffHeapError(String message, Throwable cause) {
			super(message, cause);
		}
		public OffHeapError(String message) {
			super(message);
		}
	}
	
	static class OffHeapOutOfMemoryError extends OffHeapError {
		public OffHeapOutOfMemoryError(String message) {
			super(message);
		}
	}

	static void throwOutOfMemoryError(String error) {
		throw new OffHeapOutOfMemoryError(error);
	}
}
