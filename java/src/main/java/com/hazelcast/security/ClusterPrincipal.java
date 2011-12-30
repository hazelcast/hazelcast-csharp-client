package com.hazelcast.security;

import java.io.DataInput;
import java.io.DataOutput;
import java.io.IOException;
import java.security.Principal;

import com.hazelcast.nio.DataSerializable;
import com.hazelcast.nio.SerializationHelper;

public final class ClusterPrincipal implements Principal, DataSerializable {
	
	private Credentials credentials;

	public ClusterPrincipal() {
		super();
	}
	
	public ClusterPrincipal(Credentials credentials) {
		super();
		this.credentials = credentials;
	}
	
	public String getEndpoint() {
		return credentials != null ? credentials.getEndpoint() : null;
	}
	
	public String getPrincipal() {
		return credentials != null ? credentials.getPrincipal() : null;
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

	public void writeData(DataOutput out) throws IOException {
		SerializationHelper.writeObject(out, credentials);
	}

	public void readData(DataInput in) throws IOException {
		credentials = (Credentials) SerializationHelper.readObject(in);
	}
}
