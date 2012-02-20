package com.hazelcast.elasticmemory.error;

public class OffHeapOutOfMemoryError extends OutOfMemoryError {

    public OffHeapOutOfMemoryError(String message) {
        super(message);
    }
}