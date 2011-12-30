package com.hazelcast.elasticmemory.storage;

import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;

public interface StorageFactory {
	
	static final ILogger logger = Logger.getLogger(StorageFactory.class.getName());
	
	Storage createStorage();
	
}
