package com.hazelcast.security;

public final class SecurityConstants {

	public static final String DEFAULT_CONFIGURATION_CLASS = "com.hazelcast.security.impl.DefaultLoginConfigurationImpl";
	public static final String DEFAULT_POLICY_CLASS = "com.hazelcast.security.impl.DefaultClusterPolicyImpl";
	
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
	
}
