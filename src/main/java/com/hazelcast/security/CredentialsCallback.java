package com.hazelcast.security;

import javax.security.auth.callback.Callback;

/**
 *  CredentialsCallback is used to retrieve {@link Credentials}.
 *  It is passed to {@link ClusterCallbackHandler} 
 *  and used by {@link javax.security.auth.spi.LoginModule}s 
 *  during login process.
 */
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
