package com.hazelcast.elasticmemory;

import java.util.logging.Level;

import com.hazelcast.elasticmemory.storage.OffHeapStorage;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.base.DefaultNodeInitializer;
import com.hazelcast.impl.base.NodeInitializer;
import com.hazelcast.impl.concurrentmap.DefaultRecordFactory;
import com.hazelcast.impl.concurrentmap.RecordFactory;

public class EnterpriseNodeInitializer extends DefaultNodeInitializer implements NodeInitializer {
	
	private Storage storage ;
	
	public void beforeInitialize(Node node) {
		systemLogger = node.getLogger("com.hazelcast.system");
		parseSystemProps();
		if(isOffHeapEnabled()) {
			systemLogger.log(Level.INFO, "Initializing node off-heap store...");
	        int heapSize = node.groupProperties.OFFHEAP_TOTAL_SIZE.getInteger(); // MB
	        int chunk = node.groupProperties.OFFHEAP_CHUNK_SIZE.getInteger(); // KB
	        systemLogger.log(Level.WARNING, "<<<<<<<<<< " + heapSize + " MB OFF-HEAP >>>>>>>>>>");
	        systemLogger.log(Level.WARNING, "<<<<<<<<<< " + chunk + " KB CHUNK-SIZE >>>>>>>>>>");
	        storage = new OffHeapStorage(heapSize, chunk);
		}
	}
	
	public void afterInitialize(Node node) {
        systemLogger.log(Level.INFO, "Hazelcast Enterprise " + version + " ("
                + build + ") starting at " + node.getThisAddress());
        systemLogger.log(Level.INFO, "Copyright (C) 2008-2011 Hazelcast.com");
	}
	
	public RecordFactory getRecordFactory() {
		return isOffHeapEnabled() ? new OffHeapRecordFactory(storage) : new DefaultRecordFactory();
	}

	public boolean isEnterprise() {
		// check license
		return true;
	}

	public boolean isOffHeapEnabled() {
		// enabled in config
		return true;
	}

	public Storage getOffHeapStorage() {
		return storage;
	}
	
	public int getOrder() {
		return 100;
	}
}
