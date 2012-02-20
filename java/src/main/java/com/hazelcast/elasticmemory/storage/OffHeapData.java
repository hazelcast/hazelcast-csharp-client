package com.hazelcast.elasticmemory.storage;

import com.hazelcast.nio.Data;

public class OffHeapData extends Data {

    public final static short VALID = 1;
    public final static short INVALID = -1;
    public final static OffHeapData EMPTY_OFF_HEAP_DATA = new OffHeapData(new byte[0], VALID);
    public final static OffHeapData INVALID_OFF_HEAP_DATA = new OffHeapData(null, INVALID);

    private transient short status = VALID;

    public OffHeapData() {
        super();
    }

    public OffHeapData(final byte[] value, final short status) {
        super(value);
        this.status = status;
    }

    public boolean isValid() {
        return status == VALID;
    }
}
