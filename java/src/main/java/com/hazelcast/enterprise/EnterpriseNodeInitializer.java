package com.hazelcast.enterprise;

import com.hazelcast.elasticmemory.ElasticRecordFactory;
import com.hazelcast.elasticmemory.storage.InstanceStorageFactory;
import com.hazelcast.elasticmemory.storage.SingletonStorageFactory;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.elasticmemory.storage.StorageFactory;
import com.hazelcast.impl.DefaultProxyFactory;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.ProxyFactory;
import com.hazelcast.impl.base.DefaultNodeInitializer;
import com.hazelcast.impl.base.NodeInitializer;
import com.hazelcast.impl.concurrentmap.RecordFactory;
import com.hazelcast.security.SecurityContext;
import com.hazelcast.security.SecurityContextImpl;
import com.hazelcast.security.impl.SecureProxyFactory;

import java.util.Calendar;
import java.util.Date;
import java.util.logging.Level;

public class EnterpriseNodeInitializer extends DefaultNodeInitializer implements NodeInitializer {

    private StorageFactory storageFactory;
    private Storage storage;
    private volatile License license;
    private SecurityContext securityContext;
    private boolean securityEnabled = false;

    public EnterpriseNodeInitializer() {
        super();
    }

    public void beforeInitialize(Node node) {
        this.node = node;
        logger = node.getLogger("com.hazelcast.enterprise.initializer");
        Date validUntil = null;
        try {
            logger.log(Level.INFO, "Checking Hazelcast Enterprise license...");
            String licenseKey = node.groupProperties.ENTERPRISE_LICENSE_KEY.getString();
            if (licenseKey == null || "".equals(licenseKey)) {
                licenseKey = node.getConfig().getLicenseKey();
            }
            license = KeyGenUtil.extractLicense(licenseKey != null ? licenseKey.toCharArray() : null);
            Calendar cal = Calendar.getInstance();
            cal.set(license.year, license.month, license.day, 23, 59, 59);
            validUntil = cal.getTime();
            logger.log(Level.INFO, "Licensed type: " + (license.full ? "Full" : "Trial")
                    + ", Valid until: " + validUntil
                    + ", Max nodes: " + license.nodes);
        } catch (Exception e) {
            throw new InvalidLicenseError();
        }

        if (license == null || validUntil == null ||
                System.currentTimeMillis() > validUntil.getTime()) {
            throw new TrialLicenseExpiredError();
        }

        systemLogger = node.getLogger("com.hazelcast.system");
        parseSystemProps();
        securityEnabled = node.getConfig().getSecurityConfig().isEnabled();

        simpleRecord = node.groupProperties.CONCURRENT_MAP_SIMPLE_RECORD.getBoolean();
        if (node.groupProperties.ELASTIC_MEMORY_SHARED_STORAGE.getBoolean()) {
            logger.log(Level.WARNING, "Using SingletonStorageFactory for Hazelcast Elastic Memory...");
            storageFactory = new SingletonStorageFactory();
        } else {
            storageFactory = new InstanceStorageFactory(node);
        }

        if (node.groupProperties.ELASTIC_MEMORY_ENABLED.getBoolean()) {
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
        if (license == null) {
            logger.log(Level.SEVERE, "Hazelcast Enterprise license could not be found!");
            node.shutdown(true, true);
            return;
        }
        final int count = node.getClusterImpl().getMembers().size();
        if (count > license.nodes) {
            logger.log(Level.SEVERE, "Exceeded maximum number of nodes allowed in Hazelcast Enterprise license! " +
                    "Max: " + license.nodes + ", Current: " + count);
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
        if (securityEnabled && securityContext == null) {
            securityContext = new SecurityContextImpl(node);
        }
        return securityContext;
    }

    public Storage getOffHeapStorage() {
        return storage;
    }

    public void destroy() {
        super.destroy();
        license = null;
        if (storage != null) {
            logger.log(Level.FINEST, "Destroying node off-heap storage.");
            storage.destroy();
            storage = null;
        }
    }
}
