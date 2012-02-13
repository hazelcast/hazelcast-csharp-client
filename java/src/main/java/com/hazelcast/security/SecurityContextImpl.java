package com.hazelcast.security;

import com.hazelcast.config.*;
import com.hazelcast.config.LoginModuleConfig.LoginModuleUsage;
import com.hazelcast.impl.Node;
import com.hazelcast.logging.ILogger;
import com.hazelcast.nio.Serializer;

import javax.security.auth.Subject;
import javax.security.auth.login.Configuration;
import javax.security.auth.login.LoginContext;
import javax.security.auth.login.LoginException;
import java.security.AccessControlException;
import java.security.Permission;
import java.security.PrivilegedExceptionAction;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.logging.Level;

public class SecurityContextImpl implements SecurityContext {

    private final ILogger logger;
    private final Node node;
    private final IPermissionPolicy policy;
    private final ICredentialsFactory credentialsFactory;
    private final Configuration memberConfiguration;
    private final Configuration clientConfiguration;
    private final IAccessController accessController;

    public SecurityContextImpl(Node node) {
        super();
        this.node = node;
        logger = node.getLogger("com.hazelcast.enterprise.security");
        logger.log(Level.INFO, "Initializing Hazelcast Enterprise security context.");

        SecurityConfig securityConfig = node.config.getSecurityConfig();

        PermissionPolicyConfig policyConfig = securityConfig.getClientPolicyConfig();
        if (policyConfig.getClassName() == null) {
            policyConfig.setClassName(SecurityConstants.DEFAULT_POLICY_CLASS);
        }
        IPermissionPolicy tmpPolicy = policyConfig.getImplementation();
        if (tmpPolicy == null) {
            tmpPolicy = (IPermissionPolicy) createImplInstance(policyConfig.getClassName());
        }
        policy = tmpPolicy;
        policy.configure(securityConfig, policyConfig.getProperties());

        CredentialsFactoryConfig credentialsFactoryConfig = securityConfig.getMemberCredentialsConfig();
        if (credentialsFactoryConfig.getClassName() == null) {
            credentialsFactoryConfig.setClassName(SecurityConstants.DEFAULT_CREDENTIALS_FACTORY_CLASS);
        }
        ICredentialsFactory tmpCredentialsFactory = credentialsFactoryConfig.getImplementation();
        if (tmpCredentialsFactory == null) {
            tmpCredentialsFactory = (ICredentialsFactory) createImplInstance(credentialsFactoryConfig.getClassName());
        }
        credentialsFactory = tmpCredentialsFactory;
        credentialsFactory.configure(node.config.getGroupConfig(), credentialsFactoryConfig.getProperties());

        memberConfiguration = new LoginConfigurationDelegate(node.config, getLoginModuleConfigs(securityConfig.getMemberLoginModuleConfigs()));
        clientConfiguration = new LoginConfigurationDelegate(node.config, getLoginModuleConfigs(securityConfig.getClientLoginModuleConfigs()));

        accessController = new AccessControllerImpl(this, policy);
    }

    public LoginContext createMemberLoginContext(Credentials credentials) throws LoginException {
        logger.log(Level.FINEST, "Creating Member LoginContext for: " + SecurityUtil.getCredentialsFullName(credentials));
        return new LoginContext(node.getConfig().getGroupConfig().getName(),
                new Subject(), new ClusterCallbackHandler(credentials), memberConfiguration);
    }

    public LoginContext createClientLoginContext(Credentials credentials) throws LoginException {
        logger.log(Level.FINEST, "Creating Client LoginContext for: " + SecurityUtil.getCredentialsFullName(credentials));
        return new LoginContext(node.getConfig().getGroupConfig().getName(),
                new Subject(), new ClusterCallbackHandler(credentials), clientConfiguration);
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

    private LoginModuleConfig[] getLoginModuleConfigs(final List<LoginModuleConfig> modules) {
        if (modules.isEmpty()) {
            modules.add(getDefaultLoginModuleConfig());
        }
        return modules.toArray(new LoginModuleConfig[modules.size()]);
    }

    private LoginModuleConfig getDefaultLoginModuleConfig() {
        final GroupConfig groupConfig = node.config.getGroupConfig();
        final LoginModuleConfig module = new LoginModuleConfig(SecurityConstants.DEFAULT_LOGIN_MODULE,
                LoginModuleUsage.REQUIRED);
        module.getProperties().setProperty(SecurityConstants.ATTRIBUTE_CONFIG_GROUP, groupConfig.getName());
        module.getProperties().setProperty(SecurityConstants.ATTRIBUTE_CONFIG_PASS, groupConfig.getPassword());
        return module;
    }

    public ICredentialsFactory getCredentialsFactory() {
        return credentialsFactory;
    }

    public void checkPermission(Permission permission)
            throws AccessControlException {
        accessController.checkPermission(permission);
    }

    public <T> T doAsPrivileged(Subject subject,
                                PrivilegedExceptionAction<T> action) throws Exception, SecurityException {
        return accessController.doAsPrivileged(subject, action);
    }

    public boolean checkPermission(Subject subject, Permission permission) {
        return accessController.checkPermission(subject, permission);
    }

    public <V> SecureCallable<V> createSecureCallable(Subject subject, Callable<V> callable) {
        return new SecureCallableImpl<V>(subject, callable);
    }

    public ILogger getLogger(String name) {
        return node.getLogger(name);
    }

    public void destroy() {
        logger.log(Level.INFO, "Destroying Hazelcast Enterprise security context.");
        policy.destroy();
        credentialsFactory.destroy();
    }
}
