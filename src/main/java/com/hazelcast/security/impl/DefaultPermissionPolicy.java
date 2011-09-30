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
import com.hazelcast.security.permission.ClusterPermission;
import com.hazelcast.security.permission.ClusterPermissionCollection;
import com.hazelcast.security.permission.MapPermission;
import com.hazelcast.security.permission.QueuePermission;


public class DefaultPermissionPolicy implements IPermissionPolicy {
	
	private static final ILogger logger = Logger.getLogger(DefaultPermissionPolicy.class.getName());
	
	// Configured permissions
	final ConcurrentMap<String, PermissionCollection> principalPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final ConcurrentMap<String, PermissionCollection> endpointPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final ConcurrentMap<String, PermissionCollection> dualPermissions = new ConcurrentHashMap<String, PermissionCollection>();
	final PermissionCollection allowAllPermissions = new ClusterPermissionCollection();
	
	public void configure(SecurityConfig securityConfig, Properties properties) {
		logger.log(Level.FINEST, "Configuring and initializing policy.");
		final Set<PermissionConfig> permissionConfigs = securityConfig.getPermissionConfigs();
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
		ClusterPermissionCollection coll = principal.getPermissions(type);
		if(coll == null) {
			coll = new ClusterPermissionCollection(type);
			principal.getPermissions().put(type, coll);
		}
		return coll;
	}
	
	private void ensurePrincipalPermissions(ClusterPrincipal principal) {
		if(principal != null && principal.getPermissions().isEmpty()) {
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
	};

}
