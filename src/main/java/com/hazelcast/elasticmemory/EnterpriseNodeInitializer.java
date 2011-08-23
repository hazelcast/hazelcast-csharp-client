package com.hazelcast.elasticmemory;

import java.util.logging.Level;

import com.hazelcast.elasticmemory.storage.OffHeapStorage;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.elasticmemory.util.MemoryUnit;
import com.hazelcast.elasticmemory.util.MemoryValue;
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
			
			String heapSize = node.groupProperties.OFFHEAP_TOTAL_SIZE.getValue();
	        String chunkSize = node.groupProperties.OFFHEAP_CHUNK_SIZE.getValue();
	        MemoryValue heapSizeValue = MemoryValue.parseMemoryValue(heapSize, MemoryUnit.MegaBytes);
	        MemoryValue chunkSizeValue = MemoryValue.parseMemoryValue(chunkSize, MemoryUnit.KiloBytes);
	        
	        systemLogger.log(Level.WARNING, "<<<<<<<<<< " + heapSize + " OFF-HEAP >>>>>>>>>>");
	        systemLogger.log(Level.WARNING, "<<<<<<<<<< " + chunkSize + " CHUNK-SIZE >>>>>>>>>>");
	        storage = new OffHeapStorage(heapSizeValue.megaBytes(), chunkSizeValue.kiloBytes());
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
