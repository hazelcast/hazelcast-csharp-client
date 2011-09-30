package com.hazelcast.security;

public final class SecurityConstants {

	public static final String ATTRIBUTE_CONFIG_GROUP = "com.hazelcast.config.group";
	public static final String ATTRIBUTE_CONFIG_PASS = "com.hazelcast.config.pass";
	
	public static final String DEFAULT_LOGIN_MODULE = "com.hazelcast.security.impl.DefaultLoginModuleImpl";
	public static final String DEFAULT_POLICY_CLASS = "com.hazelcast.security.impl.DefaultPermissionPolicyImpl";
	public static final String DEFAULT_CREDENTIALS_FACTORY_CLASS = "com.hazelcast.security.impl.DefaultCredentialsFactoryImpl";
	
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
	
	public static boolean nameMatches(final String name, final String pattern) {
        final int index = pattern.indexOf('*');
        if (index == -1) {
            return name.equals(pattern);
        } else {
            final String firstPart = pattern.substring(0, index);
            final int indexFirstPart = name.indexOf(firstPart, 0);
            if (indexFirstPart == -1) {
                return false;
            }
            final String secondPart = pattern.substring(index + 1);
            final int indexSecondPart = name.indexOf(secondPart, index + 1);
            return indexSecondPart != -1;
        }
    }
}
