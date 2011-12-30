package com.hazelcast.security.impl;

import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.impl.IdGeneratorProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.IdGeneratorPermission;

public class SecureIdGeneratorProxy extends SecureProxySupport implements IdGeneratorProxy {
	
	private final IdGeneratorProxy proxy;
	private final IdGeneratorPermission incPermission ;
	
	SecureIdGeneratorProxy(Node node, IdGeneratorProxy proxy) {
		super(node);
		this.proxy = proxy;
		incPermission = new IdGeneratorPermission(getName(), SecurityConstants.ACTION_INCREMENT);
	}

	public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
		proxy.setHazelcastInstance(hazelcastInstance);
	}

	public String getName() {
		return proxy.getName();
	}

	public long newId() {
		SecurityUtil.checkPermission(node.securityContext, incPermission);
		return proxy.newId();
	}

	public InstanceType getInstanceType() {
		return proxy.getInstanceType();
	}

	public void destroy() {
		SecurityUtil.checkPermission(node.securityContext, new IdGeneratorPermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public Object getId() {
		return proxy.getId();
	}
}
