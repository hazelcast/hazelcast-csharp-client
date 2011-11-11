package com.hazelcast.elasticmemory;

import static com.hazelcast.nio.IOUtil.toObject;

import com.hazelcast.elasticmemory.storage.EntryRef;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.enterprise.EnterpriseNodeInitializer;
import com.hazelcast.impl.AbstractSimpleRecord;
import com.hazelcast.impl.CMap;
import com.hazelcast.impl.Record;
import com.hazelcast.nio.Data;

public final class SimpleOffHeapRecord extends AbstractSimpleRecord implements Record {

    private volatile EntryRef entryRef;
//    private final Storage storage; 

//    public SimpleOffHeapRecord(Storage storage, CMap cmap, int blockId, Data key, Data value, long id) {
//        super(blockId, cmap, id, key);
//        this.storage = storage;
//        setValue(value);
//    }
    
    public SimpleOffHeapRecord(CMap cmap, int blockId, Data key, Data value, long id) {
        super(blockId, cmap, id, key);
        setValue(value);
    }

    public SimpleOffHeapRecord copy() {
//        return new SimpleOffHeapRecord(getStorage(), cmap, blockId, key, getValueData(), id);
    	return new SimpleOffHeapRecord(cmap, blockId, key, getValueData(), id);
    }

    public Object getValue() {
        return toObject(getValueData());
    }

    public Data getValueData() {
    	return OffHeapRecordHelper.getValue(key, entryRef, getStorage());
    }

    public void setValue(Data value) {
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
}
