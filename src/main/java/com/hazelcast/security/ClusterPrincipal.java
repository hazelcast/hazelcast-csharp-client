package com.hazelcast.security;

import java.security.Principal;

public final class ClusterPrincipal implements Principal {
	
	private final Credentials credentials;

	ClusterPrincipal(Credentials credentials) {
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
		return SecurityUtil.getCredentialsFullName(credentials);
	}

	public Credentials getCredentials() {
		return credentials;
	}
	
	@Override
	public String toString() {
		return "ClusterPrincipal [principal=" + getPrincipal() + ", endpoint=" + getEndpoint() + "]";
	}
}
