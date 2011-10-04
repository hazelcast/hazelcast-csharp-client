package com.hazelcast.security.impl;

import static com.hazelcast.security.SecurityConstants.nameMatches;

import java.security.Permission;
import java.security.PermissionCollection;
import java.security.Principal;
import java.util.Collection;
import java.util.Properties;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.logging.Level;

import javax.security.auth.Subject;

import com.hazelcast.config.PermissionConfig;
import com.hazelcast.config.SecurityConfig;
import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;
import com.hazelcast.security.ClusterPrincipal;
import com.hazelcast.security.IPermissionPolicy;
import com.hazelcast.security.permission.AllPermissions;
import com.hazelcast.security.permission.AllPermissions.AllPermissionsCollection;
import com.hazelcast.security.permission.AtomicNumberPermission;
import com.hazelcast.security.permission.ClusterPermission;
import com.hazelcast.security.permission.ClusterPermissionCollection;
import com.hazelcast.security.permission.CountDownLatchPermission;
import com.hazelcast.security.permission.DenyAllPermissionCollection;
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


public class DefaultPermissionPolicy implements IPermissionPolicy {
	
	private static final ILogger logger = Logger.getLogger(DefaultPermissionPolicy.class.getName());
	private static final PermissionCollection DENY_ALL = new DenyAllPermissionCollection();
	private static final PermissionCollection ALLOW_ALL = new AllPermissionsCollection(true);
	
	// Configured permissions
	final ConcurrentMap<String, PermissionCollection> confPrincipalPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final ConcurrentMap<String, PermissionCollection> confEndpointPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final ConcurrentMap<String, PermissionCollection> confPrincipalAndEndpointPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final PermissionCollection confPermissionsForEverbody = new ClusterPermissionCollection();
	
	// Principal permissions
	final ConcurrentMap<String, PrincipalPermissionsHolder> permissionsMap = new ConcurrentHashMap<String, PrincipalPermissionsHolder>();
	
	public void configure(SecurityConfig securityConfig, Properties properties) {
		logger.log(Level.FINEST, "Configuring and initializing policy.");
		final Set<PermissionConfig> permissionConfigs = securityConfig.getClientPermissionConfigs();
		ClusterPermission permission;
		PermissionCollection coll;
		for (PermissionConfig permCfg : permissionConfigs) {
			permission = createPermission(permCfg);
			if(permCfg.getPrincipal() != null && permCfg.getEndpoint() != null) {
				coll = getConfigPermissionCollection(confPrincipalAndEndpointPermissions, permCfg.getPrincipal() + '@' + permCfg.getEndpoint());
			} else if(permCfg.getPrincipal() != null) {
				coll = getConfigPermissionCollection(confPrincipalPermissions, permCfg.getPrincipal());
			} else if(permCfg.getEndpoint() != null) {
				coll = getConfigPermissionCollection(confEndpointPermissions, permCfg.getEndpoint());
			} else {
				coll = confPermissionsForEverbody;
			}
			coll.add(permission);
		}
	}
	
	private PermissionCollection getConfigPermissionCollection(ConcurrentMap<String, PermissionCollection> map, String key) {
		PermissionCollection coll = map.get(key);
		if(coll == null) {
			coll = new ClusterPermissionCollection();
			map.put(key, coll);
		}
		return coll;
	}
	
	public PermissionCollection getPermissions(Subject subject, Class<? extends Permission> type) {
		final ClusterPrincipal principal = getPrincipal(subject);
		if(principal == null) {
			return DENY_ALL;
		}
		
		ensurePrincipalPermissions(principal);
		final PrincipalPermissionsHolder permissionsHolder = permissionsMap.get(principal.getName());
		if(!permissionsHolder.prepared.get()) {
			synchronized (permissionsHolder) {
				if(!permissionsHolder.prepared.get()) {
					try {
						permissionsHolder.wait();
					} catch (InterruptedException ignored) {
					}
				}
			}
		}
		
		if(permissionsHolder.hasAllPermissions) {
			return ALLOW_ALL;
		}
		PermissionCollection coll = permissionsHolder.permissions.get(type);
		if(coll == null) {
			coll = DENY_ALL;
			permissionsHolder.permissions.putIfAbsent(type, coll);
		}
		return coll;
	}
	
	private ClusterPrincipal getPrincipal(Subject subject) {
		final Set<Principal> principals = subject.getPrincipals();
		for (Principal p : principals) {
			if(p instanceof ClusterPrincipal) {
				return (ClusterPrincipal) p;
			}
		}
		return null;
	}
	
	private void ensurePrincipalPermissions(ClusterPrincipal principal) {
		if(principal != null) {
			final String principalName = principal.getName();
			if(!permissionsMap.containsKey(principalName)) {
				final PrincipalPermissionsHolder permissionsHolder = new PrincipalPermissionsHolder();
				if(permissionsMap.putIfAbsent(principalName, permissionsHolder) != null) {
					return;
				}
				
				try {
					logger.log(Level.FINEST, "Preparing permissions for: " + principalName);
					final ClusterPermissionCollection all = new ClusterPermissionCollection();
					all.add(confPermissionsForEverbody);
					
					final Set<String> names = confPrincipalAndEndpointPermissions.keySet();
					for (String name : names) {
						if(nameMatches(principalName, name)) {
							all.add(confPrincipalAndEndpointPermissions.get(name));
						}
					}
					
					final Set<String> endpoints = confEndpointPermissions.keySet();
					final String principalEndpoint = principal.getEndpoint(); 
					for (String endpoint : endpoints) {
						if(nameMatches(principalEndpoint, endpoint)) {
							all.add(confEndpointPermissions.get(endpoint));
						}
					}
					
					final PermissionCollection pc = confPrincipalPermissions.get(principal.getPrincipal());
					if(pc != null) {
						all.add(pc);
					}
					
					final Set<Permission> allPermissions = all.getPermissions();
					for (Permission perm : allPermissions) {
						if(perm instanceof AllPermissions) {
							permissionsHolder.permissions.clear();
							permissionsHolder.hasAllPermissions = true;
							logger.log(Level.FINEST, "Granted all-permissions for: " + principalName);
							return;
						}
						Class<? extends Permission> type = perm.getClass();
						ClusterPermissionCollection coll = (ClusterPermissionCollection) permissionsHolder.permissions.get(type);
						if(coll == null) {
							coll = new ClusterPermissionCollection(type);
							permissionsHolder.permissions.put(type, coll);
						}
						coll.add(perm);
					}
					
					logger.log(Level.FINEST, "Compacting permissions for: " + principalName);
					final Collection<PermissionCollection> principalCollections = permissionsHolder.permissions.values();
					for (PermissionCollection coll : principalCollections) {
						((ClusterPermissionCollection) coll).compact();
					}
					
				} finally {
					synchronized (permissionsHolder) {
						permissionsHolder.prepared.set(true);
						permissionsHolder.notifyAll();
					}
				}
			}
		}
	}

	private ClusterPermission createPermission(PermissionConfig permissionConfig) {
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
	
	private class PrincipalPermissionsHolder {
		final AtomicBoolean prepared = new AtomicBoolean(false);
		boolean hasAllPermissions = false;
		final ConcurrentMap<Class<? extends Permission>, PermissionCollection> permissions = 
			new ConcurrentHashMap<Class<? extends Permission>, PermissionCollection>();
	}

	public void destroy() {
		permissionsMap.clear();
		confEndpointPermissions.clear();
		confPrincipalAndEndpointPermissions.clear();
		confPrincipalPermissions.clear();
	}
}
