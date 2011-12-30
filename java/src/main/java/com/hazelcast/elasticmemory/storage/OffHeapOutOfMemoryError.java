package com.hazelcast.elasticmemory.storage;

public class OffHeapOutOfMemoryError extends OutOfMemoryError {

	public OffHeapOutOfMemoryError(String message) {
		super(message);
	}
}