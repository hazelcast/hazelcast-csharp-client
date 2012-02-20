package com.hazelcast.elasticmemory;

import com.hazelcast.elasticmemory.storage.EntryRef;
import com.hazelcast.elasticmemory.storage.OffHeapData;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.enterprise.EnterpriseNodeInitializer;
import com.hazelcast.impl.AbstractRecord;
import com.hazelcast.impl.CMap;
import com.hazelcast.impl.DefaultRecord;
import com.hazelcast.impl.Record;
import com.hazelcast.impl.base.DistributedLock;
import com.hazelcast.impl.concurrentmap.ValueHolder;
import com.hazelcast.nio.Data;

import static com.hazelcast.nio.IOUtil.toData;
import static com.hazelcast.nio.IOUtil.toObject;

public final class OffHeapRecord extends AbstractRecord implements Record {

    private volatile EntryRef entryRef;

    public OffHeapRecord(CMap cmap, int blockId, Data key, Data value, long ttl, long maxIdleMillis, long id) {
        super(cmap, blockId, key, ttl, maxIdleMillis, id);
        setValueData(value);
    }

    public Record copy() {
        Record recordCopy = new DefaultRecord(cmap, blockId, key, getValueData(), getRemainingTTL(), getRemainingIdle(), id);
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

    protected void invalidateValueCache() {
    }

    public Data getValueData() {
        final EntryRef ref = entryRef;
        OffHeapData value = OffHeapRecordHelper.getValue(key, ref, getStorage());
        if (value != null) {
            if (value.isValid()) {
                return value;
            } else {
                getValueData();
            }
        }
        return null;
    }

    public void setValueData(Data value) {
//    	invalidateValueCache();
        entryRef = OffHeapRecordHelper.setValue(key, entryRef, value, getStorage());
    }

    public Object getValue() {
        return toObject(getValueData());
    }

    public Object setValue(Object value) {
        //FIXME: for semaphore
        setValueData(toData(value));
        return null;
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

    private Storage getStorage() {
        return ((EnterpriseNodeInitializer) cmap.getNode().initializer).getOffHeapStorage();
    }

    public void invalidate() {
        OffHeapRecordHelper.removeValue(key, entryRef, getStorage());
    }
}
