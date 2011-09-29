package com.hazelcast.security;

import javax.security.auth.callback.Callback;

public class CredentialsCallback implements Callback {
	
	private Credentials credentials;
	
	public CredentialsCallback() {
	}
	
	public void setCredentials(Credentials credentials) {
		this.credentials = credentials;
	}
	
	public Credentials getCredentials() {
		return credentials;
	}

}
