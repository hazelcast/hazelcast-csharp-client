package com.hazelcast.security;

import java.security.Principal;
import java.util.Map;
import java.util.logging.Level;

import javax.security.auth.Subject;
import javax.security.auth.callback.Callback;
import javax.security.auth.callback.CallbackHandler;
import javax.security.auth.login.LoginException;
import javax.security.auth.spi.LoginModule;

import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;

public abstract class ClusterLoginModule implements LoginModule {

	protected final ILogger logger = Logger.getLogger(getClass().getName());
	private CallbackHandler callbackHandler;
	protected Credentials credentials;
	protected Subject subject;
	protected Map options ;
	protected Map sharedState;
	protected boolean loginSucceeded = false;
	protected boolean commitSucceeded = false;

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
		if(credentials == null) {
			logger.log(Level.WARNING, "Credentials could not be retrieved!");
			return false;
		}
		logger.log(Level.FINEST, "Authenticating " + SecurityUtil.getCredentialsFullName(credentials));
		sharedState.put(SecurityConstants.ATTRIBUTE_CREDENTIALS, credentials);
		return loginSucceeded = onLogin();
	}
	
	
	public final boolean commit() throws LoginException {
		if(!loginSucceeded) {
			logger.log(Level.WARNING, "Authentication has been failed! =>" + (credentials != null 
					? SecurityUtil.getCredentialsFullName(credentials) : "unknown"));
			return false;
		}
		logger.log(Level.FINEST, "Committing authentication of " + SecurityUtil.getCredentialsFullName(credentials));
		final Principal principal = new ClusterPrincipal(credentials);
		subject.getPrincipals().add(principal);
		sharedState.put(SecurityConstants.ATTRIBUTE_PRINCIPAL, principal);
		return commitSucceeded = onCommit();
	}

	public final boolean abort() throws LoginException {
		logger.log(Level.FINEST, "Aborting authentication of " + SecurityUtil.getCredentialsFullName(credentials));
		final boolean abort = onAbort();
		clearSubject();
		loginSucceeded = false;
		commitSucceeded = false;
		return abort;
	}

	public final boolean logout() throws LoginException {
		logger.log(Level.FINEST, "Logging out " + SecurityUtil.getCredentialsFullName(credentials));
		final boolean logout = onLogout();
		clearSubject();
		loginSucceeded = false;
		commitSucceeded = false;
		return logout;
	}
	
	private void clearSubject() {
		subject.getPrincipals().clear();
		subject.getPrivateCredentials().clear();
		subject.getPublicCredentials().clear();
	}
	
	protected abstract boolean onLogin() throws LoginException;
	protected abstract boolean onCommit() throws LoginException;
	protected abstract boolean onAbort() throws LoginException;
	protected abstract boolean onLogout() throws LoginException;

}
