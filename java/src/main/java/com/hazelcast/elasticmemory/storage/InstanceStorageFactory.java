package com.hazelcast.elasticmemory.storage;

import com.hazelcast.cluster.NodeAware;
import com.hazelcast.impl.Node;

import java.util.logging.Level;

public class InstanceStorageFactory extends StorageFactorySupport implements StorageFactory {

    final Node node;

    public InstanceStorageFactory(Node node) {
        super();
        this.node = node;
    }

    public Storage createStorage() {
        String total = node.groupProperties.ELASTIC_MEMORY_TOTAL_SIZE.getValue();
        logger.log(Level.FINEST, "Read " + node.groupProperties.ELASTIC_MEMORY_TOTAL_SIZE.getName() + " as: " + total);
        String chunk = node.groupProperties.ELASTIC_MEMORY_CHUNK_SIZE.getValue();
        logger.log(Level.FINEST, "Read " + node.groupProperties.ELASTIC_MEMORY_CHUNK_SIZE.getName() + " as: " + chunk);
        Storage storage = createStorage(total, chunk);
        if (storage instanceof NodeAware) {
            ((NodeAware) storage).setNode(node);
        }
        return storage;
    }

}
