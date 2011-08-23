package com.hazelcast.elasticmemory;

import static com.hazelcast.nio.IOUtil.toObject;

import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.impl.AbstractRecord;
import com.hazelcast.impl.CMap;
import com.hazelcast.impl.Record;
import com.hazelcast.impl.base.DistributedLock;
import com.hazelcast.nio.Data;

public final class OffHeapRecord extends AbstractRecord implements Record {

    private volatile EntryRef entryRef;
    private final Storage storage; 

    public OffHeapRecord(Storage storage, CMap cmap, int blockId, Data key, Data value, long ttl, long maxIdleMillis, Long id) {
        super(cmap, blockId, key, null, ttl, maxIdleMillis, id);
        this.storage = storage;
        setValue(value);
    }

    public OffHeapRecord copy() {
        OffHeapRecord recordCopy = new OffHeapRecord(storage, cmap, blockId, key, getValueData(), getRemainingTTL(), getRemainingIdle(), id);
        if (optionalInfo != null) {
            recordCopy.setIndexes(getIndexes(), getIndexTypes());
            recordCopy.setMultiValues(getMultiValues());
        }
        if (lock != null) {
            recordCopy.setLock(new DistributedLock(lock));
        }
        recordCopy.setVersion(getVersion());
        return recordCopy;
    }

    public Object getValue() {
        return toObject(getValueData());
    }

    public Data getValueData() {
    	return new Data(storage.get(key.getPartitionHash(), entryRef));
    }

    public Object setValue(Object value) {
        return null;
    }

    public void setValue(Data value) {
    	if(value == null || storage == null) {
    		return;
    	}
        invalidateValueCache();
        storage.remove(key.getPartitionHash(), entryRef);
        if(value != null && value.buffer != null) {
        	entryRef = storage.put(key.getPartitionHash(), value.buffer);
        }
        else {
        	entryRef = null;
        }
    }

    public int valueCount() {
        int count = 0;
        if (hasValueData()) {
            count = 1;
        } else if (getMultiValues() != null) {
            count = getMultiValues().size();
        }
        return count;
    }

    public long getCost() {
        long cost = 0;
        // avoid race condition with local references
        final Data dataKey = getKeyData();
        if (hasValueData()) {
            cost = entryRef.length;
        } else if (getMultiValues() != null && getMultiValues().size() > 0) {
            for (Data data : getMultiValues()) {
                if (data != null) {
                    cost += data.size();
                }
            }
        }
        return cost + dataKey.size() + 312;
    }

	public boolean hasValueData() {
		return entryRef != null;
	}
}
