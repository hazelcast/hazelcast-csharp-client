package com.hazelcast.security;

import java.util.Map;

import javax.security.auth.login.AppConfigurationEntry;
import javax.security.auth.login.Configuration;

public final class LoginConfigurationDelegate extends Configuration {
	
	private ILoginConfiguration config;
	private Map settings;
	
	public LoginConfigurationDelegate(ILoginConfiguration config, Map settings) {
		super();
		this.config = config;
		this.settings = settings;
	}

	public AppConfigurationEntry[] getAppConfigurationEntry(String name) {
		return config.getConfigurationEntries(settings);
	}

	public void refresh() {
		config.refresh(settings);
	}

}
