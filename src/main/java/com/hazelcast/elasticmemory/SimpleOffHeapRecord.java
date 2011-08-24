package com.hazelcast.elasticmemory;

import static com.hazelcast.nio.IOUtil.toObject;

import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.impl.AbstractSimpleRecord;
import com.hazelcast.impl.CMap;
import com.hazelcast.impl.Record;
import com.hazelcast.nio.Data;

public final class SimpleOffHeapRecord extends AbstractSimpleRecord implements Record {

    private volatile EntryRef entryRef;
    private final Storage storage; 

    public SimpleOffHeapRecord(Storage storage, CMap cmap, int blockId, Data key, Data value, long id) {
        super(blockId, cmap, id, key);
        this.storage = storage;
        setValue(value);
    }

    public SimpleOffHeapRecord copy() {
        return new SimpleOffHeapRecord(storage, cmap, blockId, key, getValueData(), id);
    }

    public Object getValue() {
        return toObject(getValueData());
    }

    public Data getValueData() {
    	return new Data(storage.get(key.getPartitionHash(), entryRef));
    }

    public void setValue(Data value) {
    	if(value == null || storage == null) {
    		return;
    	}
        storage.remove(key.getPartitionHash(), entryRef);
        if(value != null && value.buffer != null) {
        	entryRef = storage.put(key.getPartitionHash(), value.buffer);
        }
        else {
        	entryRef = null;
        }
    }

    public long getCost() {
        long cost = 0;
        // avoid race condition with local references
        final Data dataKey = getKeyData();
        if (hasValueData()) {
            cost = entryRef.length;
        }
        return cost + dataKey.size() + 30;
    }

	public boolean hasValueData() {
		return entryRef != null;
	}

	public Object setValue(Object value) {
		return null;
	}

	public int valueCount() {
		return 1;
	}

	public boolean containsValue(Data value) {
		return false;
	}

	public void addValue(Data value) {
	}
}
