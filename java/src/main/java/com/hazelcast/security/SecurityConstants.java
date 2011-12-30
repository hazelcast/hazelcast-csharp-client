package com.hazelcast.security;


public final class SecurityConstants {

	public static final String ATTRIBUTE_CONFIG = "com.hazelcast.config";
	public static final String ATTRIBUTE_CONFIG_GROUP = "com.hazelcast.config.group";
	public static final String ATTRIBUTE_CONFIG_PASS = "com.hazelcast.config.pass";
	public static final String ATTRIBUTE_CREDENTIALS = "com.hazelcast.security.credentials";
	public static final String ATTRIBUTE_PRINCIPAL = "com.hazelcast.security.principal";
	
	public static final String DEFAULT_LOGIN_MODULE = "com.hazelcast.security.impl.DefaultLoginModule";
	public static final String DEFAULT_POLICY_CLASS = "com.hazelcast.security.impl.DefaultPermissionPolicy";
	public static final String DEFAULT_CREDENTIALS_FACTORY_CLASS = "com.hazelcast.security.impl.DefaultCredentialsFactory";
	
	public static final String ACTION_ALL = "all";
	public static final String ACTION_CREATE = "create";
	public static final String ACTION_DESTROY = "destroy";
	public static final String ACTION_PUT = "put";
	public static final String ACTION_ADD = "add";
	public static final String ACTION_GET = "get";
	public static final String ACTION_SET = "set";
	public static final String ACTION_REMOVE = "remove";
	public static final String ACTION_OFFER = "offer";
	public static final String ACTION_POLL = "poll";
	public static final String ACTION_TAKE = "take";
	public static final String ACTION_LOCK = "lock";
	public static final String ACTION_LISTEN = "listen";
	public static final String ACTION_PUBLISH = "publish";
	public static final String ACTION_INCREMENT = "increment";
	public static final String ACTION_DECREMENT = "decrement";
	public static final String ACTION_EXECUTE = "execute";
	public static final String ACTION_COUNTDOWN = "countdown";
	public static final String ACTION_ACQUIRE = "acquire";
	public static final String ACTION_RELEASE = "release";
	public static final String ACTION_DRAIN = "drain";
	public static final String ACTION_STATISTICS = "statistics";
	
	public static final String LISTENER_INSTANCE = "instance";
	public static final String LISTENER_MEMBER = "member";
	public static final String LISTENER_MIGRATION = "migration";
}
