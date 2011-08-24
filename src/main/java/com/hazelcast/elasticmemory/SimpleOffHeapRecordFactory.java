package com.hazelcast.elasticmemory;

import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.impl.CMap;
import com.hazelcast.impl.Record;
import com.hazelcast.impl.concurrentmap.RecordFactory;
import com.hazelcast.nio.Data;

public class SimpleOffHeapRecordFactory implements RecordFactory {
	
	final Storage storage; 
	
	public SimpleOffHeapRecordFactory(Storage storage) {
		super();
		this.storage = storage;
	}

	public Record createNewRecord(CMap cmap, int blockId, Data key, Data value,
			long ttl, long maxIdleMillis, long id) {
		return new SimpleOffHeapRecord(storage, cmap, blockId, key, value, id);
	}
}
