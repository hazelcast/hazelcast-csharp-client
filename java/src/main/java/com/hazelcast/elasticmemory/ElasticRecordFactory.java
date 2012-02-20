package com.hazelcast.elasticmemory;

import com.hazelcast.config.MapConfig;
import com.hazelcast.config.MapConfig.StorageType;
import com.hazelcast.elasticmemory.error.OffHeapError;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.impl.CMap;
import com.hazelcast.impl.GroupProperties;
import com.hazelcast.impl.NearCacheRecord;
import com.hazelcast.impl.Record;
import com.hazelcast.impl.concurrentmap.DefaultRecordFactory;
import com.hazelcast.impl.concurrentmap.RecordFactory;
import com.hazelcast.nio.Data;

public class ElasticRecordFactory extends DefaultRecordFactory implements RecordFactory {

    final Storage storage;
    final boolean offheapEnabled;

    public ElasticRecordFactory(Storage storage, boolean simple) {
        super(simple);
        this.storage = storage;
        this.offheapEnabled = storage != null;
    }

    public Record createNewRecord(CMap cmap, int blockId, Data key, Data value,
                                  long ttl, long maxIdleMillis, long id) {
        if (isOffHeapRecord(cmap.getMapConfig())) {
            if (simple) {
                return new SimpleOffHeapRecord(cmap, blockId, key, value, id);
            }
            return new OffHeapRecord(cmap, blockId, key, value, ttl, maxIdleMillis, id);
        }
        return super.createNewRecord(cmap, blockId, key, value, ttl, maxIdleMillis, id);
    }

    public NearCacheRecord createNewNearCacheRecord(CMap cmap, Data key, Data value) {
        if (isOffHeapRecord(cmap.getMapConfig())) {
            return new OffHeapNearCacheRecord(storage, key, value);
        }
        return super.createNewNearCacheRecord(cmap, key, value);
    }

    private boolean isOffHeapRecord(MapConfig mapConfig) {
        final StorageType storageType = mapConfig.getStorageType();
        final boolean offHeapMap = storageType == StorageType.OFFHEAP;
        if (!offheapEnabled && offHeapMap) {
            throw new OffHeapError("Hazelcast Elastic Memory is not enabled! (Set '"
                    + GroupProperties.PROP_ELASTIC_MEMORY_ENABLED + "' to true.)");
        }
        return offheapEnabled && (storageType == null || offHeapMap);
    }
}
