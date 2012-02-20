package com.hazelcast.elasticmemory;

import com.hazelcast.elasticmemory.storage.EntryRef;
import com.hazelcast.elasticmemory.storage.OffHeapData;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.enterprise.EnterpriseNodeInitializer;
import com.hazelcast.impl.AbstractSimpleRecord;
import com.hazelcast.impl.CMap;
import com.hazelcast.impl.Record;
import com.hazelcast.impl.SimpleRecord;
import com.hazelcast.nio.Data;

import static com.hazelcast.nio.IOUtil.toObject;

public final class SimpleOffHeapRecord extends AbstractSimpleRecord implements Record {

    private volatile EntryRef entryRef;

    public SimpleOffHeapRecord(CMap cmap, int blockId, Data key, Data value, long id) {
        super(blockId, cmap, id, key);
        setValueData(value);
    }

    public Record copy() {
        return new SimpleRecord(blockId, cmap, id, key, getValueData());
    }

    public Object getValue() {
        return toObject(getValueData());
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
        entryRef = OffHeapRecordHelper.setValue(key, entryRef, value, getStorage());
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

    private Storage getStorage() {
        return ((EnterpriseNodeInitializer) cmap.getNode().initializer).getOffHeapStorage();
    }

    public void invalidate() {
        OffHeapRecordHelper.removeValue(key, entryRef, getStorage());
    }
}
