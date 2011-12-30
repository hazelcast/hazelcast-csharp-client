package com.hazelcast.elasticmemory.storage;

import java.util.logging.Level;

import com.hazelcast.impl.Node;

public class NodeStorageFactory extends StorageFactorySupport implements StorageFactory {

	final Node node;
	
	public NodeStorageFactory(Node node) {
		super();
		this.node = node;
	}

	public Storage createStorage() {
		String total = node.groupProperties.ELASTIC_MEMORY_TOTAL_SIZE.getValue();
		logger.log(Level.FINEST, "Read " + node.groupProperties.ELASTIC_MEMORY_TOTAL_SIZE.getName() + " as: " + total);
        String chunk = node.groupProperties.ELASTIC_MEMORY_CHUNK_SIZE.getValue();
        logger.log(Level.FINEST, "Read " + node.groupProperties.ELASTIC_MEMORY_CHUNK_SIZE.getName() + " as: " + chunk);
        return createStorage(total, chunk);
	}
	
}
