package com.hazelcast.enterprise;

import java.util.logging.Level;

import com.hazelcast.elasticmemory.OffHeapRecordFactory;
import com.hazelcast.elasticmemory.SimpleOffHeapRecordFactory;
import com.hazelcast.elasticmemory.storage.OffHeapStorage;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.elasticmemory.util.MemorySize;
import com.hazelcast.elasticmemory.util.MemoryUnit;
import com.hazelcast.impl.IHazelcastFactory;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.base.DefaultNodeInitializer;
import com.hazelcast.impl.base.NodeInitializer;
import com.hazelcast.impl.concurrentmap.RecordFactory;
import com.hazelcast.security.SecurityContext;
import com.hazelcast.security.SecurityContextImpl;
import com.hazelcast.security.impl.SecureHazelcastFactory;

public class EnterpriseNodeInitializer extends DefaultNodeInitializer implements NodeInitializer {
	
	private Storage storage ;
	private Registration registration;
	protected Node node;
	
	public EnterpriseNodeInitializer() {
		super();
	}
	
	public void beforeInitialize(Node node) {
		this.node = node;
		logger = node.getLogger("com.hazelcast.enterprise.initializer");
		try {
			logger.log(Level.INFO, "Checking Hazelcast Enterprise license...");
			registration = RegistrationService.getRegistration(); 
			logger.log(Level.INFO, "Licensed to: " + registration.getOwner() + " on " + registration.getRegistryDate() 
					+ ", type: " + registration.getType());
		} catch (Exception e) {
			logger.log(Level.WARNING, e.getMessage(), e);
			throw new InvalidLicenseError();
		}
		
		if(!isRegistered()) {
			throw new TrialLicenseExpiredError();
		}
		
		systemLogger = node.getLogger("com.hazelcast.system");
		parseSystemProps();
		simpleRecord = node.groupProperties.CONCURRENT_MAP_SIMPLE_RECORD.getBoolean();
		if(isOffHeapEnabled()) {
			systemLogger.log(Level.INFO, "Initializing node off-heap store...");
			
			String heap = node.groupProperties.OFFHEAP_TOTAL_SIZE.getValue();
	        String chunk = node.groupProperties.OFFHEAP_CHUNK_SIZE.getValue();
	        MemorySize heapSize = MemorySize.parse(heap, MemoryUnit.MEGABYTES);
	        MemorySize chunkSize = MemorySize.parse(chunk, MemoryUnit.KILOBYTES);
	        
	        systemLogger.log(Level.WARNING, "<<<<<<<<<< " + heap + " OFF-HEAP >>>>>>>>>>");
	        systemLogger.log(Level.WARNING, "<<<<<<<<<< " + chunk + " CHUNK-SIZE >>>>>>>>>>");
	        storage = new OffHeapStorage(heapSize.megaBytes(), chunkSize.kiloBytes());
		}
	}
	
	public void afterInitialize(Node node) {
        systemLogger.log(Level.INFO, "Hazelcast Enterprise Edition " + version + " ("
                + build + ") starting at " + node.getThisAddress());
        systemLogger.log(Level.INFO, "Copyright (C) 2008-2011 Hazelcast.com");
	}
	
	public RecordFactory getRecordFactory() {
		return isOffHeapEnabled() ? 
				(simpleRecord ? new SimpleOffHeapRecordFactory(storage) : new OffHeapRecordFactory(storage)) 
				: super.getRecordFactory();
	}
	
	public SecurityContext createSecurityContext() {
		return new SecurityContextImpl(node);
	}

	public IHazelcastFactory getSecureHazelcastFactory() {
		return new SecureHazelcastFactory(node);
	}
	
	public boolean isRegistered() {
		return registration != null && registration.isValid();
	}

	public boolean isOffHeapEnabled() {
		return node.groupProperties.OFFHEAP_ENABLED.getBoolean();
	}

	public Storage getOffHeapStorage() {
		return storage;
	}
}
