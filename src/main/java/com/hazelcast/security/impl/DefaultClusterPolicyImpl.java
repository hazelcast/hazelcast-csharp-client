package com.hazelcast.security.impl;

import java.security.Permission;
import java.security.PermissionCollection;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import javax.security.auth.Subject;

import com.hazelcast.config.SecurityConfig;
import com.hazelcast.config.SecurityConfig.PermissionConfig;
import com.hazelcast.security.IClusterPolicy;
import com.hazelcast.security.permission.ClusterPermissionCollection;
import com.hazelcast.security.permission.MapPermission;
import com.hazelcast.security.permission.QueuePermission;

public class DefaultClusterPolicyImpl implements IClusterPolicy {
	
	final ConcurrentMap<String, PermissionCollection> principalPermissionMap = new ConcurrentHashMap<String, PermissionCollection>();
	final ConcurrentMap<String, PermissionCollection> endpointPermissionMap = new ConcurrentHashMap<String, PermissionCollection>();

	public void configure(SecurityConfig securityConfig) {
		final Set<PermissionConfig> permissionConfigs = securityConfig.getPermissionConfigs();
		for (PermissionConfig permCfg : permissionConfigs) {
			
		}
	}
	
	public PermissionCollection getAllPermissions(Subject subject) {
		PermissionCollection p = new ClusterPermissionCollection();
		p.add(new MapPermission("*", "get", "put"));
		p.add(new QueuePermission("*", "offer", "poll"));
		return p;
	}

	public PermissionCollection getPermissions(Subject subject, Permission type) {
		return getAllPermissions(subject);
	}

}
