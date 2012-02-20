package com.hazelcast.security.impl;

import com.hazelcast.security.ClusterLoginModule;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.UsernamePasswordCredentials;

import javax.security.auth.login.LoginException;
import javax.security.auth.spi.LoginModule;

public class DefaultLoginModule extends ClusterLoginModule implements LoginModule {

    public boolean onLogin() throws LoginException {
        if (credentials instanceof UsernamePasswordCredentials) {
            final UsernamePasswordCredentials usernamePasswordCredentials = (UsernamePasswordCredentials) credentials;
            final String group = (String) options.get(SecurityConstants.ATTRIBUTE_CONFIG_GROUP);
            final String pass = (String) options.get(SecurityConstants.ATTRIBUTE_CONFIG_PASS);

            if (!group.equals(usernamePasswordCredentials.getUsername())) {
                return false;
            }

            if (!pass.equals(new String(usernamePasswordCredentials.getRawPassword()))) {
                return false;
            }
            return true;
        }
        return false;
    }

    public boolean onCommit() throws LoginException {
        return loginSucceeded;
    }

    protected boolean onAbort() throws LoginException {
        return true;
    }

    protected boolean onLogout() throws LoginException {
        return true;
    }

}
