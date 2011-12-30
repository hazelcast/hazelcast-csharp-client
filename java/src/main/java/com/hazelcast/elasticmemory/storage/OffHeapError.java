package com.hazelcast.elasticmemory.storage;

public class OffHeapError extends Error {
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
