package com.hazelcast.elasticmemory.storage;

import com.hazelcast.impl.GroupProperties;
import com.hazelcast.nio.Data;

import java.util.logging.Level;

public class SingletonStorageFactory extends StorageFactorySupport implements StorageFactory {

    private static Storage STORAGE = null;
    private static int REF_COUNT = 0;
    private static final Object MUTEX = SingletonStorageFactory.class;

    public SingletonStorageFactory() {
        super();
    }

    public Storage createStorage() {
        synchronized (MUTEX) {
            if (STORAGE == null) {
                initStorage();
            }
            REF_COUNT++;
            return new StorageProxy(STORAGE);
        }
    }

    private class StorageProxy implements Storage {
        Storage storage;

        StorageProxy(Storage storage) {
            super();
            this.storage = storage;
        }

        public EntryRef put(int hash, Data data) {
            return storage.put(hash, data);
        }

        public OffHeapData get(int hash, EntryRef entry) {
            return storage.get(hash, entry);
        }

        public void remove(int hash, EntryRef entry) {
            storage.remove(hash, entry);
        }

        public void destroy() {
            synchronized (MUTEX) {
                if (storage != null) {
                    storage = null;
                    destroyStorage();
                }
            }
        }
    }

    private static void initStorage() {
        synchronized (MUTEX) {
            if (STORAGE != null) {
                throw new IllegalStateException("Storage is already initialized!");
            }
            final String total = System.getProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE);
            final String chunk = System.getProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE);
            if (total == null || chunk == null) {
                throw new IllegalArgumentException("Both '" + GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE
                        + "' and '" + GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE
                        + "' system properties are mandatory!");
            }

            logger.log(Level.FINEST, "Read " + GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE + " as: " + total);
            logger.log(Level.FINEST, "Read " + GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE + " as: " + chunk);
            logger.log(Level.INFO, "Initializing Singleton Storage...");
            STORAGE = createStorage(total, chunk);
        }
    }

    private static void destroyStorage() {
        synchronized (MUTEX) {
            if (STORAGE != null) {
                if (REF_COUNT <= 0) {
                    logger.log(Level.SEVERE, "Storage reference count is invalid: " + REF_COUNT);
                    REF_COUNT = 1;
                }
                REF_COUNT--;
                if (REF_COUNT == 0) {
                    logger.log(Level.INFO, "Destroying Singleton Storage ...");
                    STORAGE.destroy();
                    STORAGE = null;
                }
            } else {
                logger.log(Level.WARNING, "Storage is already destroyed !");
                if (REF_COUNT != 0) {
                    final String errorMsg = "Storage reference count must be zero (0), but it is not !!!";
                    logger.log(Level.SEVERE, errorMsg);
                    throw new IllegalStateException(errorMsg);
                }
            }
        }
    }
}
