package com.hazelcast.security;

import java.security.AccessControlException;
import java.security.Permission;
import java.security.PrivilegedAction;
import java.util.HashMap;
import java.util.Map;

import javax.security.auth.Subject;
import javax.security.auth.login.Configuration;
import javax.security.auth.login.LoginContext;
import javax.security.auth.login.LoginException;

import com.hazelcast.config.SecurityConfig;
import com.hazelcast.impl.Node;
import com.hazelcast.nio.Serializer;

public class SecurityContextImpl implements SecurityContext {
	
	private final Node node;
	private final IClusterPolicy policy;
	private final Configuration configuration;
	private final IAccessController accessController;
	
	public SecurityContextImpl(Node node) {
		super();
		this.node = node;
		
		SecurityConfig securityConfig = node.config.getSecurityConfig();
		if(securityConfig.getPolicyClassName() == null) {
			securityConfig.setPolicyClassName(SecurityConstants.DEFAULT_POLICY_CLASS);
		}
		IClusterPolicy clusterPolicy = securityConfig.getPolicyImpl();
		if(clusterPolicy == null) {
			clusterPolicy = (IClusterPolicy) createImplInstance(securityConfig.getPolicyClassName());
		}
		clusterPolicy.configure(securityConfig);
		
		if(securityConfig.getLoginConfigurationClassName() == null) {
			securityConfig.setLoginConfigurationClassName(SecurityConstants.DEFAULT_CONFIGURATION_CLASS);
		}
		ILoginConfiguration loginConfig = securityConfig.getLoginConfigurationImpl();
		if(loginConfig == null) {
			loginConfig = (ILoginConfiguration) createImplInstance(securityConfig.getLoginConfigurationClassName());
		}
		
		policy = clusterPolicy;
		Map settings = new HashMap(securityConfig.getProperties());
		settings.put(ILoginConfiguration.ATTRIBUTE_CONFIG, node.getConfig());
		settings.put(ILoginConfiguration.ATTRIBUTE_SECURITY, this);
		configuration = new LoginConfigurationDelegate(loginConfig, settings);
		accessController = new AccessControllerImpl(policy);
	}
	
	public LoginContext createLoginContext(Credentials credentials) throws LoginException {
		return new LoginContext(node.getConfig().getGroupConfig().getName(), 
				new Subject(), new ClusterCallbackHandler(credentials), configuration);
	}

	public IAccessController getAccessController() {
		return accessController;
	}
	
	public IClusterPolicy getPolicy() {
		return policy;
	}
	
	private Object createImplInstance(final String className) {
		try {
			Class klass = Serializer.loadClass(className);
			return Serializer.newInstance(klass);
		} catch (Exception e) {
			throw new IllegalArgumentException("Could not create instance of '" + className 
					+ "', cause: " + e.getMessage(), e);
		}
	}

	public void checkPermission(Permission permission)
			throws AccessControlException {
		accessController.checkPermission(permission);
	}

	public <T> T doAsPrivileged(Subject subject,
			PrivilegedAction<T> action) throws SecurityException {
		return accessController.doAsPrivileged(subject, action);
	}
	
	public boolean checkPermission(Subject subject, Permission permission) {
		return accessController.checkPermission(subject, permission);
	}
}
