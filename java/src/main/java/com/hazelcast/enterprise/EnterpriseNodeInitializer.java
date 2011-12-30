package com.hazelcast.enterprise;

import java.util.logging.Level;

import com.hazelcast.elasticmemory.ElasticRecordFactory;
import com.hazelcast.elasticmemory.storage.NodeStorageFactory;
import com.hazelcast.elasticmemory.storage.SingletonStorageFactory;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.elasticmemory.storage.StorageFactory;
import com.hazelcast.enterprise.Registration.Mode;
import com.hazelcast.impl.DefaultProxyFactory;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.ProxyFactory;
import com.hazelcast.impl.base.DefaultNodeInitializer;
import com.hazelcast.impl.base.NodeInitializer;
import com.hazelcast.impl.concurrentmap.RecordFactory;
import com.hazelcast.security.SecurityContext;
import com.hazelcast.security.SecurityContextImpl;
import com.hazelcast.security.impl.SecureProxyFactory;

public class EnterpriseNodeInitializer extends DefaultNodeInitializer implements NodeInitializer {
	
	private StorageFactory storageFactory;
	private Storage storage ;
	private Registration registration;
	private SecurityContext securityContext;
	private boolean securityEnabled = false;
	
	public EnterpriseNodeInitializer() {
		super();
	}
	
	public void beforeInitialize(Node node) {
		this.node = node;
		logger = node.getLogger("com.hazelcast.enterprise.initializer");
		try {
			logger.log(Level.INFO, "Checking Hazelcast Enterprise license...");
			registration = RegistrationService.getRegistration(node.groupProperties.LICENSE_PATH.getString(), logger); 
			logger.log(Level.INFO, "Licensed to: " + registration.getOwner() 
					+ (registration.getMode() == Mode.TRIAL ? " until " + registration.getExpiryDate() : "") 
					+ ", Max-Nodes: " + registration.getMaxNodes()
					+ ", Type: " + registration.getMode());
		} catch (Exception e) {
			logger.log(Level.WARNING, e.getMessage(), e);
			throw new InvalidLicenseError();
		}
		
		if(!isRegistered()) {
			throw new TrialLicenseExpiredError();
		}
		
		systemLogger = node.getLogger("com.hazelcast.system");
		parseSystemProps();
		securityEnabled = node.getConfig().getSecurityConfig().isEnabled();
		
		simpleRecord = node.groupProperties.CONCURRENT_MAP_SIMPLE_RECORD.getBoolean();
		if(node.groupProperties.ELASTIC_MEMORY_SHARED_STORAGE.getBoolean()) {
			logger.log(Level.WARNING, "Using SingletonStorageFactory for Hazelcast Elastic Memory...");
			storageFactory = new SingletonStorageFactory();
		} else {
			storageFactory = new NodeStorageFactory(node);
		}
		
		if(node.groupProperties.ELASTIC_MEMORY_ENABLED.getBoolean()) {
			logger.log(Level.INFO, "Initializing node off-heap storage.");
	        storage = storageFactory.createStorage();
		}
	}
	
	public void printNodeInfo(Node node) {
        systemLogger.log(Level.INFO, "Hazelcast Enterprise Edition " + version + " ("
                + build + ") starting at " + node.getThisAddress());
        systemLogger.log(Level.INFO, "Copyright (C) 2008-2011 Hazelcast.com");
	}
	
	public void afterInitialize(Node node) {
		final int count = node.getClusterImpl().getMembers().size();
		if(count > registration.getMaxNodes()) {
			logger.log(Level.SEVERE, "Exceeded maximum number of nodes allowed in Hazelcast Enterprise license! " +
					"Max: " + registration.getMaxNodes() + ", Current: " + count);
			node.shutdown(true, true);
		}
    }
	
	public ProxyFactory getProxyFactory() {
    	return securityEnabled ? new SecureProxyFactory(node) : new DefaultProxyFactory(node.factory);
    }
	
	public RecordFactory getRecordFactory() {
		return new ElasticRecordFactory(storage, simpleRecord); 
	}
	
	public SecurityContext getSecurityContext() {
		if(securityEnabled && securityContext == null) {
			securityContext = new SecurityContextImpl(node); 
		}
		return securityContext;
	}
	
	private boolean isRegistered() {
		return registration != null && registration.isValid();
	}

	public Storage getOffHeapStorage() {
		return storage;
	}
	
	public void destroy() {
		super.destroy();
		registration = null;
		if(storage != null) {
			logger.log(Level.FINEST, "Destroying node off-heap storage.");
			storage.destroy();
			storage = null;
		}
	}
}
