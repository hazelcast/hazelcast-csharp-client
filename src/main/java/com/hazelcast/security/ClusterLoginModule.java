package com.hazelcast.security;

import java.security.Principal;
import java.util.Map;
import java.util.Set;
import java.util.logging.Level;

import javax.security.auth.Subject;
import javax.security.auth.callback.Callback;
import javax.security.auth.callback.CallbackHandler;
import javax.security.auth.login.LoginException;
import javax.security.auth.spi.LoginModule;

import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;

public abstract class ClusterLoginModule implements LoginModule {

	protected static final ILogger logger = Logger.getLogger(ClusterLoginModule.class.getName());
	
	private CallbackHandler callbackHandler;
	protected Credentials credentials;
	protected Subject subject;
	protected Map<String, ?> options ;
	protected Map<String, ?> sharedState;

	public final void initialize(Subject subject, CallbackHandler callbackHandler,
			Map<String, ?> sharedState, Map<String, ?> options) {
		this.subject = subject;
		this.callbackHandler = callbackHandler;
		this.sharedState = sharedState;
		this.options = options;
	}

	public final boolean login() throws LoginException {
		final CredentialsCallback cb = new CredentialsCallback();
		try {
			callbackHandler.handle(new Callback[]{cb});
			credentials = cb.getCredentials();
		} catch (Exception e) {
			throw new LoginException(e.getClass().getName() + ":" + e.getMessage());
		}
		logger.log(Level.FINEST, "Authenticating " + credentials.getName());
		return onLogin();
	}
	
	
	public final boolean commit() throws LoginException {
		logger.log(Level.FINEST, "Committing authentication of " + credentials.getName());
		subject.getPrincipals().add(new ClusterPrincipal(credentials));
		return onCommit();
	}

	public final boolean abort() throws LoginException {
		logger.log(Level.FINEST, "Aborting authentication of " + credentials.getName());
		clearSubject();
		return onAbort();
	}

	public final boolean logout() throws LoginException {
		logger.log(Level.FINEST, "Logging out " + credentials.getName());
		clearSubject();
		return onLogout();
	}
	
	private void clearSubject() {
		final Set<Principal> principals = subject.getPrincipals();
		for (Principal p : principals) {
			if(p instanceof ClusterPrincipal) {
				((ClusterPrincipal) p).getPermissions().clear();
			}
		}
		principals.clear();
		subject.getPrivateCredentials().clear();
		subject.getPublicCredentials().clear();
	}
	
	protected abstract boolean onLogin() throws LoginException;
	protected abstract boolean onCommit() throws LoginException;
	protected abstract boolean onAbort() throws LoginException;
	protected abstract boolean onLogout() throws LoginException;

}
