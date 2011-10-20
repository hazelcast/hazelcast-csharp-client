package com.hazelcast.security;

import java.util.Arrays;

import com.hazelcast.config.Config;
import com.hazelcast.config.PermissionConfig;
import com.hazelcast.impl.AddressPicker;
import com.hazelcast.security.permission.AllPermissions;
import com.hazelcast.security.permission.AtomicNumberPermission;
import com.hazelcast.security.permission.ClusterPermission;
import com.hazelcast.security.permission.CountDownLatchPermission;
import com.hazelcast.security.permission.ExecutorServicePermission;
import com.hazelcast.security.permission.ListPermission;
import com.hazelcast.security.permission.ListenerPermission;
import com.hazelcast.security.permission.LockPermission;
import com.hazelcast.security.permission.MapPermission;
import com.hazelcast.security.permission.MultiMapPermission;
import com.hazelcast.security.permission.QueuePermission;
import com.hazelcast.security.permission.SemaphorePermission;
import com.hazelcast.security.permission.SetPermission;
import com.hazelcast.security.permission.TopicPermission;
import com.hazelcast.security.permission.TransactionPermission;

public final class SecurityUtil {

	public static ClusterPermission createPermission(PermissionConfig permissionConfig) {
		final String[] actions = permissionConfig.getActions().toArray(new String[0]);
		switch (permissionConfig.getType()) {
		case MAP:
			return new MapPermission(permissionConfig.getName(), actions);

		case QUEUE:
			return new QueuePermission(permissionConfig.getName(), actions);
			
		case ATOMIC_NUMBER:
			return new AtomicNumberPermission(permissionConfig.getName(), actions);
			
		case COUNTDOWN_LATCH:
			return new CountDownLatchPermission(permissionConfig.getName(), actions);
			
		case EXECUTOR_SERVICE:
			return new ExecutorServicePermission(permissionConfig.getName(), actions);
			
		case LIST:
			return new ListPermission(permissionConfig.getName(), actions);
			
		case LOCK:
			return new LockPermission(permissionConfig.getName(), actions);
		
		case MULTIMAP:
			return new MultiMapPermission(permissionConfig.getName(), actions);
			
		case SEMAPHORE:
			return new SemaphorePermission(permissionConfig.getName(), actions);
			
		case SET: 
			return new SetPermission(permissionConfig.getName(), actions);
			
		case TOPIC:
			return new TopicPermission(permissionConfig.getName(), actions);
			
		case LISTENER:
			return new ListenerPermission(permissionConfig.getName());
			
		case TRANSACTION:
			return new TransactionPermission();
			
		case ALL:
			return new AllPermissions();
			
		default:
			throw new IllegalArgumentException(permissionConfig.getType().toString());
		}
	}
	
	public static boolean nameMatches(final String name, final String pattern) {
		return Config.nameMatches(name, pattern);
    }
	
	public static boolean addressMatches(final String address, final String pattern) {
		return AddressPicker.matchAddress(address, Arrays.asList(pattern));
	}
	
	public static String getCredentialsFullName(Credentials credentials) {
		if(credentials == null) {
			return null;
		}
		return credentials.getPrincipal() + '@' + credentials.getEndpoint();
	}
	
	private SecurityUtil() {}
}
