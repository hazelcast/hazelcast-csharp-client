package com.hazelcast.elasticmemory;

import java.util.logging.Level;

import com.hazelcast.elasticmemory.enterprise.InvalidLicenseError;
import com.hazelcast.elasticmemory.enterprise.Registration;
import com.hazelcast.elasticmemory.enterprise.Registration.Type;
import com.hazelcast.elasticmemory.enterprise.RegistrationService;
import com.hazelcast.elasticmemory.storage.OffHeapStorage;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.elasticmemory.util.MemoryUnit;
import com.hazelcast.elasticmemory.util.MemoryValue;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.base.DefaultNodeInitializer;
import com.hazelcast.impl.base.NodeInitializer;
import com.hazelcast.impl.concurrentmap.RecordFactory;
import com.hazelcast.logging.ILogger;

public class EnterpriseNodeInitializer extends DefaultNodeInitializer implements NodeInitializer {
	
	private ILogger logger;
	private Storage storage ;
	private Registration registration; 
	
	public EnterpriseNodeInitializer() {
		super();
	}
	
	public void beforeInitialize(Node node) {
		logger = node.getLogger("com.hazelcast.elasticmemory.initializer");
		try {
			logger.log(Level.INFO, "Checking Hazelcast Elastic Memory license...");
			registration = RegistrationService.getRegistration(); 
			logger.log(Level.INFO, "Licensed to: " + registration.getOwner() + " on " + registration.getRegistryDate() 
					+ ", type: " + registration.getType());
			
		} catch (Exception e) {
			logger.log(Level.WARNING, e.getMessage(), e);
			throw new InvalidLicenseError();
		}
		
		systemLogger = node.getLogger("com.hazelcast.system");
		parseSystemProps();
		simpleRecord = node.groupProperties.CONCURRENT_MAP_SIMPLE_RECORD.getBoolean();
		if(isOffHeapEnabled()) {
			systemLogger.log(Level.INFO, "Initializing node off-heap store...");
			
			String heapSize = node.groupProperties.OFFHEAP_TOTAL_SIZE.getValue();
	        String chunkSize = node.groupProperties.OFFHEAP_CHUNK_SIZE.getValue();
	        MemoryValue heapSizeValue = MemoryValue.parseMemoryValue(heapSize, MemoryUnit.MEGABYTES);
	        MemoryValue chunkSizeValue = MemoryValue.parseMemoryValue(chunkSize, MemoryUnit.KILOBYTES);
	        
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
		return isOffHeapEnabled() ? 
				(simpleRecord ? new SimpleOffHeapRecordFactory(storage) : new OffHeapRecordFactory(storage)) 
				: super.getRecordFactory();
	}

	public boolean isEnterprise() {
		return registration != null && registration.isValid()
			&& (registration.getType() == Type.FULL 
					|| registration.getType() == Type.TRIAL && !registration.isExpired());
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
