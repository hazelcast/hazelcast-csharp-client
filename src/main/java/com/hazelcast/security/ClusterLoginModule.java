package com.hazelcast.security;

import java.util.Map;

import javax.security.auth.Subject;
import javax.security.auth.callback.Callback;
import javax.security.auth.callback.CallbackHandler;
import javax.security.auth.login.LoginException;
import javax.security.auth.spi.LoginModule;

import com.hazelcast.config.Config;

public abstract class ClusterLoginModule implements LoginModule {
	
	private CallbackHandler callbackHandler;
	
	protected Credentials credentials;
	protected Subject subject;
	protected SecurityContext context;
	protected Config config;
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
		context = (SecurityContext) options.get(ILoginConfiguration.ATTRIBUTE_SECURITY);
		config = (Config) options.get(ILoginConfiguration.ATTRIBUTE_CONFIG);
		final CredentialsCallback cb = new CredentialsCallback();
		try {
			callbackHandler.handle(new Callback[]{cb});
			credentials = cb.getCredentials();
		} catch (Exception e) {
			throw new LoginException(e.getClass().getName() + ":" + e.getMessage());
		}
		return onLogin();
	}
	
	
	public final boolean commit() throws LoginException {
		subject.getPrincipals().add(new ClusterPrincipal(credentials));
		return onCommit();
	}

	public final boolean abort() throws LoginException {
		clearSubject();
		return onAbort();
	}

	public final boolean logout() throws LoginException {
		clearSubject();
		return onLogout();
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
