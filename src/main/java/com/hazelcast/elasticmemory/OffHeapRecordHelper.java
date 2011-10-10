package com.hazelcast.elasticmemory;

import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.nio.Data;

final class OffHeapRecordHelper {

	private static final Data EMPTY_DATA = new Data();

	static EntryRef setValue(final Data key, final EntryRef oldEntryRef, final Data value, final Storage storage) {
		removeValue(key, oldEntryRef, storage);
    	if(storage == null) {
    		return null;
    	}
        if(value != null && value.buffer != null) {
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
		if (entryRef != null && entryRef.length > 0 && storage != null) {
			final byte[] data = storage.get(key.getPartitionHash(), entryRef);
			if (data != null && data.length > 0) {
				return new Data(data);
			}
		}
		return EMPTY_DATA;
	}
	
	private OffHeapRecordHelper() {}
}
