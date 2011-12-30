package com.hazelcast.security.impl;

import java.util.Properties;

import com.hazelcast.config.GroupConfig;
import com.hazelcast.security.Credentials;
import com.hazelcast.security.ICredentialsFactory;
import com.hazelcast.security.UsernamePasswordCredentials;

public class DefaultCredentialsFactory implements ICredentialsFactory {

	private Credentials credentials ;
	
	public void configure(GroupConfig groupConfig, Properties properties) {
		credentials = new UsernamePasswordCredentials(groupConfig.getName(), groupConfig.getPassword());
	}

	public Credentials newCredentials() {
		return credentials;
	}

	public void destroy() {
	}
}
