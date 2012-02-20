package com.hazelcast.elasticmemory;

import com.hazelcast.elasticmemory.storage.EntryRef;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.impl.NearCacheRecord;
import com.hazelcast.nio.Data;

import static com.hazelcast.nio.IOUtil.toObject;

public class OffHeapNearCacheRecord implements NearCacheRecord {

    private final Storage storage;
    private volatile EntryRef entryRef;
    private final Data keyData;

    public OffHeapNearCacheRecord(Storage storage, Data keyData, Data valueData) {
        super();
        this.keyData = keyData;
        this.storage = storage;
        setValueData(valueData);
    }

    public void setValueData(Data valueData) {
        entryRef = OffHeapRecordHelper.setValue(keyData, entryRef, valueData, storage);
    }

    public Data getValueData() {
        return OffHeapRecordHelper.getValue(keyData, entryRef, storage);
    }

    public Data getKeyData() {
        return keyData;
    }

    public boolean hasValueData() {
        return entryRef != null;
    }

    public Object getValue() {
        return toObject(getValueData());
    }

    public void invalidate() {
        OffHeapRecordHelper.removeValue(keyData, entryRef, storage);
    }
}
