package com.hazelcast.elasticmemory;

import com.hazelcast.elasticmemory.storage.EntryRef;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.impl.Constants;
import com.hazelcast.nio.Data;

final class OffHeapRecordHelper {

	static EntryRef setValue(final Data key, final EntryRef oldEntryRef, final Data value, final Storage storage) {
		if(storage == null) {
			return null;
		}
		removeValue(key, oldEntryRef, storage);
        if(value != null) {
        	if(value.buffer == null || value.buffer.length == 0) {
        		return EntryRef.EMPTY_DATA_REF;
        	}
        	return storage.put(key.getPartitionHash(), value.buffer);
        }
        return null;
    }
	
	static void removeValue(final Data key, final EntryRef entryRef, final Storage storage) {
    	if(storage == null) {
    		return;
    	}
        if(entryRef != null && entryRef.length > 0) {
        	storage.remove(key.getPartitionHash(), entryRef);
        }
    }
	
	static Data getValue(final Data key, final EntryRef entryRef, final Storage storage) {
		if(entryRef == EntryRef.EMPTY_DATA_REF) {
			return Constants.IO.EMPTY_DATA;
		}
		if (entryRef != null && entryRef.length > 0 && storage != null) {
			final byte[] data = storage.get(key.getPartitionHash(), entryRef);
			if (data != null && data.length > 0) {
				return new Data(data);
			}
		}
		return null;
	}
	
	private OffHeapRecordHelper() {}
}
