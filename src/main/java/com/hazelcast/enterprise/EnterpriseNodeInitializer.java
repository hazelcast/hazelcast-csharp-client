package com.hazelcast.enterprise;

import java.lang.management.ManagementFactory;
import java.lang.management.RuntimeMXBean;
import java.util.List;
import java.util.logging.Level;

import com.hazelcast.elasticmemory.EnterpriseRecordFactory;
import com.hazelcast.elasticmemory.storage.OffHeapStorage;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.elasticmemory.util.MathUtil;
import com.hazelcast.elasticmemory.util.MemorySize;
import com.hazelcast.elasticmemory.util.MemoryUnit;
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
	
	private static final String MAX_DIRECT_MEMORY_PARAM = "-XX:MaxDirectMemorySize";
	
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
		if(node.groupProperties.ELASTIC_MEMORY_ENABLED.getBoolean()) {
			systemLogger.log(Level.INFO, "Initializing node off-heap store...");
			
			final MemorySize jvmSize = getJvmDirectMemorySize();
			if(jvmSize == null) {
				throw new IllegalArgumentException("JVM max direct memory size argument (" + 
						MAX_DIRECT_MEMORY_PARAM + ") should be configured in order to use " +
						"Hazelcast Elastic Memory! " +
						"(Ex: java " + MAX_DIRECT_MEMORY_PARAM + "=1G -Xmx1G -cp ...)");
			}
			
			String total = node.groupProperties.ELASTIC_MEMORY_TOTAL_SIZE.getValue();
			logger.log(Level.FINEST, "Read " + node.groupProperties.ELASTIC_MEMORY_TOTAL_SIZE.getName() + " as: " + total);
	        String chunk = node.groupProperties.ELASTIC_MEMORY_CHUNK_SIZE.getValue();
	        logger.log(Level.FINEST, "Read " + node.groupProperties.ELASTIC_MEMORY_CHUNK_SIZE.getName() + " as: " + chunk);
	        final MemorySize totalSize = MemorySize.parse(total, MemoryUnit.MEGABYTES);
	        final MemorySize chunkSize = MemorySize.parse(chunk, MemoryUnit.KILOBYTES);
	        
	        checkOffHeapParams(jvmSize, totalSize, chunkSize);
	        
	        logger.log(Level.INFO, "Elastic-Memory off-heap storage total size: " + totalSize.megaBytes() + " MB");
	        logger.log(Level.INFO, "Elastic-Memory off-heap storage chunk size: " + chunkSize.kiloBytes() + " KB");
	        storage = new OffHeapStorage((int) totalSize.megaBytes(), (int) chunkSize.kiloBytes());
		}
	}
	
	private void checkOffHeapParams(MemorySize jvm, MemorySize total, MemorySize chunk) {
		if(jvm.megaBytes() == 0) {
			throw new IllegalArgumentException(MAX_DIRECT_MEMORY_PARAM + " must be multitude of megabytes! (Current: " 
					+ jvm.bytes() + " bytes)");
		}
		if(total.megaBytes() == 0) {
			throw new IllegalArgumentException("Elastic Memory total size must be multitude of megabytes! (Current: " 
					+ total.bytes() + " bytes)");
		}
		if(total.megaBytes() > jvm.megaBytes()) {
			throw new IllegalArgumentException(MAX_DIRECT_MEMORY_PARAM + " must be greater than or equal to Elastic Memory total size => " 
					+ MAX_DIRECT_MEMORY_PARAM + ": " + jvm.megaBytes() + " megabytes, Total: " + total.megaBytes() + " megabytes");
		}
		if(chunk.kiloBytes() == 0) {
			throw new IllegalArgumentException("Elastic Memory chunk size must be multitude of kilobytes! (Current: " 
					+ chunk.bytes() + " bytes)");
		}
		if(total.bytes() <= chunk.bytes()) {
			throw new IllegalArgumentException("Elastic Memory total size must be greater than chunk size => " 
					+ "Total: " + total.bytes() + " bytes, Chunk: " + chunk.bytes() + " bytes");
		}
		if(!MathUtil.isPowerOf2(chunk.kiloBytes())) {
			throw new IllegalArgumentException("Elastic Memory chunk size must be power of 2 in kilobytes! (Current: " 
					+ chunk.kiloBytes() + " kilobytes)");
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
		return new EnterpriseRecordFactory(storage, simpleRecord); 
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

	private MemorySize getJvmDirectMemorySize() {
		RuntimeMXBean rmx = ManagementFactory.getRuntimeMXBean();
		List<String> args = rmx.getInputArguments();
		for (String arg : args) {
			if(arg.startsWith(MAX_DIRECT_MEMORY_PARAM)) {
				logger.log(Level.FINEST, "Read JVM " + MAX_DIRECT_MEMORY_PARAM + " as: " + arg);
				String[] tmp = arg.split("\\=");
				if(tmp.length == 2) {
					final String value = tmp[1];
					return MemorySize.parse(value);
				}
				break;
			}
		}
		return null;
	}
	
	public Storage getOffHeapStorage() {
		return storage;
	}
}
