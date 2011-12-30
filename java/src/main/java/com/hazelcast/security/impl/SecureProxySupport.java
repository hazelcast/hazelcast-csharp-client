package com.hazelcast.security.impl;

import java.security.AccessControlException;
import java.security.Permission;

import com.hazelcast.impl.Node;
import com.hazelcast.security.SecurityUtil;

abstract class SecureProxySupport {

	final Node node;

	SecureProxySupport(Node node) {
		super();
		this.node = node;
	}
	
//	void checkPermission(Permission p) throws AccessControlException {
//		SecurityUtil.checkPermission(node.securityContext, p);
//	}
	
}
