package com.hazelcast.security;

import java.util.HashMap;

import javax.security.auth.login.AppConfigurationEntry;
import javax.security.auth.login.AppConfigurationEntry.LoginModuleControlFlag;
import javax.security.auth.login.Configuration;

import com.hazelcast.config.LoginModuleConfig;

public final class LoginConfigurationDelegate extends Configuration {
	
	private final LoginModuleConfig[] loginModuleConfigs;
	
	public LoginConfigurationDelegate(LoginModuleConfig[] loginModuleConfigs) {
		super();
		this.loginModuleConfigs = loginModuleConfigs;
	}

	public AppConfigurationEntry[] getAppConfigurationEntry(String name) {
		final AppConfigurationEntry[] entries = new AppConfigurationEntry[loginModuleConfigs.length];
		for (int i = 0; i < loginModuleConfigs.length; i++) {
			final LoginModuleConfig module = loginModuleConfigs[i];
			LoginModuleControlFlag flag = getFlag(module);
			entries[i] = new AppConfigurationEntry(module.getClassName(), 
					flag, new HashMap(module.getProperties()));
		}
		return entries;
	}
	
	private LoginModuleControlFlag getFlag(LoginModuleConfig module) {
		switch (module.getUsage()) {
		case REQUIRED:
			return LoginModuleControlFlag.REQUIRED;
			
		case OPTIONAL:
			return LoginModuleControlFlag.OPTIONAL;
			
		case REQUISITE:
			return LoginModuleControlFlag.REQUISITE;
			
		case SUFFICIENT:
			return LoginModuleControlFlag.SUFFICIENT;

		default:
			return LoginModuleControlFlag.REQUIRED;
		}
	}

	public void refresh() {
	}

}
