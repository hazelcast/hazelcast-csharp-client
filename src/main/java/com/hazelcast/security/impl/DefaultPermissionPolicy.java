package com.hazelcast.security.impl;

import static com.hazelcast.security.SecurityConstants.nameMatches;

import java.security.Permission;
import java.security.PermissionCollection;
import java.security.Principal;
import java.util.Collection;
import java.util.Enumeration;
import java.util.Map;
import java.util.Properties;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.logging.Level;

import javax.security.auth.Subject;

import com.hazelcast.config.PermissionConfig;
import com.hazelcast.config.SecurityConfig;
import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;
import com.hazelcast.security.ClusterPrincipal;
import com.hazelcast.security.IPermissionPolicy;
import com.hazelcast.security.permission.AllPermissions;
import com.hazelcast.security.permission.AtomicNumberPermission;
import com.hazelcast.security.permission.ClusterPermission;
import com.hazelcast.security.permission.ClusterPermissionCollection;
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


public class DefaultPermissionPolicy implements IPermissionPolicy {
	
	private static final ILogger logger = Logger.getLogger(DefaultPermissionPolicy.class.getName());
	
	// Configured permissions
	final ConcurrentMap<String, PermissionCollection> principalPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final ConcurrentMap<String, PermissionCollection> endpointPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final ConcurrentMap<String, PermissionCollection> dualPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final PermissionCollection allowAllPermissions = new ClusterPermissionCollection();
	
	public void configure(SecurityConfig securityConfig, Properties properties) {
		logger.log(Level.FINEST, "Configuring and initializing policy.");
		final Set<PermissionConfig> permissionConfigs = securityConfig.getClientPermissionConfigs();
		ClusterPermission permission;
		PermissionCollection coll;
		for (PermissionConfig permCfg : permissionConfigs) {
			permission = createPermission(permCfg);
			if(permCfg.getPrincipal() != null && permCfg.getEndpoint() != null) {
				coll = getPermissionCollection(dualPermissions, permCfg.getPrincipal() + '@' + permCfg.getEndpoint());
			} else if(permCfg.getPrincipal() != null) {
				coll = getPermissionCollection(principalPermissions, permCfg.getPrincipal());
			} else if(permCfg.getEndpoint() != null) {
				coll = getPermissionCollection(endpointPermissions, permCfg.getEndpoint());
			} else {
				coll = allowAllPermissions;
			}
			coll.add(permission);
		}
	}
	
	private PermissionCollection getPermissionCollection(ConcurrentMap<String, PermissionCollection> map, String key) {
		PermissionCollection coll = dualPermissions.get(key);
		if(coll == null) {
			coll = new ClusterPermissionCollection();
			dualPermissions.put(key, coll);
		}
		return coll;
	}
	
	public PermissionCollection getPermissions(Subject subject, Class<? extends Permission> type) {
		final ClusterPrincipal principal = getPrincipal(subject);
		if(principal == null) {
			return DENY_ALL;
		}
		
		ensurePrincipalPermissions(principal);
		return getPermissions(principal, type);
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
	
	private PermissionCollection getPermissions(ClusterPrincipal principal, Class<? extends Permission> type) {
		if(principal.isHasAllPermissions()) {
			return ALLOW_ALL;
		}
		ClusterPermissionCollection coll = principal.getPermissions(type);
		if(coll == null) {
			coll = new ClusterPermissionCollection(type);
			principal.getPermissions().put(type, coll);
		}
		return coll;
	}
	
	private void ensurePrincipalPermissions(ClusterPrincipal principal) {
		if(principal != null && !principal.isHasAllPermissions() && principal.getPermissions().isEmpty()) {
			final String principalName = principal.getName();
			logger.log(Level.FINEST, "Preparing permissions for: " + principalName);
			final ClusterPermissionCollection all = new ClusterPermissionCollection();
			all.add(allowAllPermissions);
			
			final Set<String> names = dualPermissions.keySet();
			for (String name : names) {
				if(nameMatches(principalName, name)) {
					all.add(dualPermissions.get(name));
				}
			}
			
			final Set<String> endpoints = endpointPermissions.keySet();
			final String principalEndpoint = principal.getEndpoint(); 
			for (String endpoint : endpoints) {
				if(nameMatches(principalEndpoint, endpoint)) {
					all.add(endpointPermissions.get(endpoint));
				}
			}
			
			final PermissionCollection pc = principalPermissions.get(principal.getPrincipal());
			if(pc != null) {
				all.add(pc);
			}
			
			final Set<Permission> allPermissions = all.getPermissions();
			final Map<Class<? extends Permission>, ClusterPermissionCollection> permissions = principal.getPermissions();
			for (Permission perm : allPermissions) {
				if(perm instanceof AllPermissions) {
					permissions.clear();
					principal.setHasAllPermissions(true);
					logger.log(Level.FINEST, "Granted all-permissions for: " + principalName);
					return;
				}
				Class<? extends Permission> type = perm.getClass();
				ClusterPermissionCollection coll = permissions.get(type);
				if(coll == null) {
					coll = new ClusterPermissionCollection(type);
					permissions.put(type, coll);
				}
				coll.add(perm);
			}
			
			logger.log(Level.FINEST, "Compacting permissions for: " + principalName);
			final Collection<ClusterPermissionCollection> principalCollections = permissions.values();
			for (ClusterPermissionCollection coll : principalCollections) {
				coll.cleanupOverlaps();
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
	
	private static final PermissionCollection DENY_ALL = new PermissionCollection() {
		public boolean implies(Permission permission) {
			return false;
		}
		public Enumeration<Permission> elements() {
			return null;
		}
		public void add(Permission permission) {
		}
		public String toString() {
			return "<deny all permissions>";
		}
	};
	
	private static final PermissionCollection ALLOW_ALL = new PermissionCollection() {
		public boolean implies(Permission permission) {
			return true;
		}
		public Enumeration<Permission> elements() {
			return null;
		}
		public void add(Permission permission) {
		}
		public String toString() {
			return "<allow all permissions>";
		}
	};
	
}
