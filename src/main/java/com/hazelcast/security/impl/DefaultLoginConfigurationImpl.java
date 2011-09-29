package com.hazelcast.security.impl;

import java.util.Map;

import javax.security.auth.login.AppConfigurationEntry;
import javax.security.auth.login.AppConfigurationEntry.LoginModuleControlFlag;

import com.hazelcast.security.ILoginConfiguration;

public class DefaultLoginConfigurationImpl implements ILoginConfiguration {

	public AppConfigurationEntry[] getConfigurationEntries(Map settings) {
		return new AppConfigurationEntry[]{
				new AppConfigurationEntry(DefaultLoginModuleImpl.class.getName(), 
						LoginModuleControlFlag.REQUIRED, settings)
		};
	}

	public void refresh(Map settings) {
	}
}
