package com.hazelcast.elasticmemory;

import static com.hazelcast.nio.IOUtil.*;

import com.hazelcast.elasticmemory.storage.EntryRef;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.enterprise.EnterpriseNodeInitializer;
import com.hazelcast.impl.AbstractRecord;
import com.hazelcast.impl.CMap;
import com.hazelcast.impl.Record;
import com.hazelcast.impl.base.DistributedLock;
import com.hazelcast.impl.concurrentmap.ValueHolder;
import com.hazelcast.nio.Data;

public final class OffHeapRecord extends AbstractRecord implements Record {

    private volatile EntryRef entryRef;
//    private final Storage storage; 

//    public OffHeapRecord(Storage storage, CMap cmap, int blockId, Data key, Data value, long ttl, long maxIdleMillis, long id) {
//        super(cmap, blockId, key, ttl, maxIdleMillis, id);
//        this.storage = storage;
//        setValue(value);
//    }

    public OffHeapRecord(CMap cmap, int blockId, Data key, Data value, long ttl, long maxIdleMillis, long id) {
        super(cmap, blockId, key, ttl, maxIdleMillis, id);
        setValue(value);
    }
    
    public OffHeapRecord copy() {
//        OffHeapRecord recordCopy = new OffHeapRecord(getStorage(), cmap, blockId, key, getValueData(), getRemainingTTL(), getRemainingIdle(), id);
    	OffHeapRecord recordCopy = new OffHeapRecord(cmap, blockId, key, getValueData(), getRemainingTTL(), getRemainingIdle(), id);
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
    	return OffHeapRecordHelper.getValue(key, entryRef, getStorage());
    }

    public Object setValue(Object value) {
    	//FIXME: for semaphore
    	setValue(toData(value));
        return null;
    }

    @Override
    protected void invalidateValueCache() {
    }
    
    public void setValue(Data value) {
//    	invalidateValueCache();
    	entryRef = OffHeapRecordHelper.setValue(key, entryRef, value, getStorage());
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
        final EntryRef entry = entryRef;
        final Data dataKey = getKeyData();
        if (entry != null) { // hasValueData()
            cost = entry.length;
        } else if (getMultiValues() != null && getMultiValues().size() > 0) {
        	for (ValueHolder valueHolder : getMultiValues()) {
                if (valueHolder != null) {
                    cost += valueHolder.getData().size();
                }
            }
        }
        return cost + dataKey.size() + 312;
    }

	public boolean hasValueData() {
		return entryRef != null;
	}
	
//	private Storage getStorage() {
//		return storage;
//	}
	
	private Storage getStorage() {
		return ((EnterpriseNodeInitializer) cmap.getNode().initializer).getOffHeapStorage();
	}
	
	public void invalidate() {
		OffHeapRecordHelper.removeValue(key, entryRef, getStorage());
	}
}
