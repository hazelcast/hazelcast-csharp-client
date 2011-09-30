package com.hazelcast.security;

import java.security.Permission;
import java.security.Principal;
import java.util.HashMap;
import java.util.Map;

import com.hazelcast.security.permission.ClusterPermissionCollection;

public final class ClusterPrincipal implements Principal, IPermissionHolder {
	
	private final Credentials credentials;
	private final Map<Class<? extends Permission>, ClusterPermissionCollection> permissions ;

	ClusterPrincipal(Credentials credentials) {
		super();
		this.credentials = credentials;
		this.permissions = new HashMap<Class<? extends Permission>, ClusterPermissionCollection>();
	}
	
	public String getEndpoint() {
		return credentials.getEndpoint();
	}
	
	public String getPrincipal() {
		return credentials.getPrincipal();
	}
	
	public String getName() {
		return credentials.getName();
	}

	public Credentials getCredentials() {
		return credentials;
	}
	
	public Map<Class<? extends Permission>, ClusterPermissionCollection> getPermissions() {
		return permissions;
	}
	
	public ClusterPermissionCollection getPermissions(Class<? extends Permission> type) {
		return permissions.get(type);
	}
	
	@Override
	public String toString() {
		return "ClusterPrincipal [principal=" + getName() + ", endpoint=" + getEndpoint() + "]";
	}
}
