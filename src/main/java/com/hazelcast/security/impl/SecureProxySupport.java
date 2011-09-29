package com.hazelcast.security.impl;

import java.security.AccessControlException;
import java.security.Permission;

import com.hazelcast.impl.Node;

abstract class SecureProxySupport {

	final Node node;

	SecureProxySupport(Node node) {
		super();
		this.node = node;
	}
	
	void checkPermission(Permission p) throws AccessControlException {
		if(node.securityContext != null) {
			node.securityContext.checkPermission(p);
		}
	}
	
}
