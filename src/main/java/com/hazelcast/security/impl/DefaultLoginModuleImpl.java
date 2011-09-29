package com.hazelcast.security.impl;

import javax.security.auth.login.LoginException;
import javax.security.auth.spi.LoginModule;

import com.hazelcast.config.GroupConfig;
import com.hazelcast.security.ClusterLoginModule;
import com.hazelcast.security.UsernamePasswordCredentials;

public class DefaultLoginModuleImpl extends ClusterLoginModule implements LoginModule {

	private UsernamePasswordCredentials usernamePasswordCredentials;
	
	public boolean onLogin() throws LoginException {
		usernamePasswordCredentials = (UsernamePasswordCredentials) credentials;
		GroupConfig groupConfig = config.getGroupConfig();
		if(!groupConfig.getName().equals(usernamePasswordCredentials.getUsername())) {
			return false;
		}
		
		if(!groupConfig.getPassword().equals(new String(usernamePasswordCredentials.getPassword()))) {
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
