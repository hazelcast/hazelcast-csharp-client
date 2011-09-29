package com.hazelcast.security;

import java.security.Principal;

public class ClusterPrincipal implements Principal {
	
	private final Credentials credentials;

	public ClusterPrincipal(Credentials credentials) {
		super();
		this.credentials = credentials;
	}
	
	public String getEndpoint() {
		return credentials.getEndpoint();
	}
	
	public String getPrincipal() {
		return credentials.getPrincipal();
	}
	
	public String getName() {
		return getPrincipal() + '@' + getEndpoint();
	}

	@Override
	public String toString() {
		return "ClusterPrincipal [principal=" + getName() + ", endpoint=" + getEndpoint() + "]";
	}
}
