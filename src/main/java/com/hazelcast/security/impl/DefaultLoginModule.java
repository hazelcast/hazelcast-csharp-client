package com.hazelcast.security.impl;

import javax.security.auth.login.LoginException;
import javax.security.auth.spi.LoginModule;

import com.hazelcast.security.ClusterLoginModule;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.UsernamePasswordCredentials;

public class DefaultLoginModule extends ClusterLoginModule implements LoginModule {

	private UsernamePasswordCredentials usernamePasswordCredentials;
	
	public boolean onLogin() throws LoginException {
		usernamePasswordCredentials = (UsernamePasswordCredentials) credentials;
		final String group = (String) options.get(SecurityConstants.ATTRIBUTE_CONFIG_GROUP);
		final String pass = (String) options.get(SecurityConstants.ATTRIBUTE_CONFIG_PASS);
		
		if(!group.equals(usernamePasswordCredentials.getUsername())) {
			return false;
		}
		
		if(!pass.equals(new String(usernamePasswordCredentials.getPassword()))) {
			return false;
		}
		return true;
	}

	public boolean onCommit() throws LoginException {
		return true;
	}

	protected boolean onAbort() throws LoginException {
		return true;
	}

	protected boolean onLogout() throws LoginException {
		return true;
	}

}
