package com.hazelcast.security.permission;

import java.security.Permission;
import java.security.PermissionCollection;
import java.util.Collections;
import java.util.Enumeration;
import java.util.HashSet;
import java.util.Iterator;
import java.util.Set;

public class ClusterPermissionCollection extends PermissionCollection {
	
	final Set<Permission> perms = new HashSet<Permission>();
	final Class<? extends Permission> permClass;
	
	public ClusterPermissionCollection() {
		super();
		permClass = null;
	}
			
	public ClusterPermissionCollection(Class<? extends Permission> permClass) {
		super();
		this.permClass = permClass;
	}

	public void add(Permission permission) {
		boolean shouldAdd = (permClass != null && permClass.equals(permission.getClass()))
			|| (permission instanceof ClusterPermission); 
			
		if(shouldAdd && !implies(permission)) {
			perms.add((ClusterPermission) permission);
		}
	}
	
	public void add(PermissionCollection permissions) {
		if(permissions instanceof ClusterPermissionCollection) {
			for (Permission p : ((ClusterPermissionCollection) permissions).perms) {
				add(p);
			}
		}
	}

	public boolean implies(Permission permission) {
		for (Permission p : perms) {
			if(p.implies(permission)) {
				return true;
			}
		}
		return false;
	}
	
	public void cleanupOverlaps() {
		final Iterator<Permission> iter = perms.iterator();
		while(iter.hasNext()) {
			final Permission perm = iter.next();
			boolean implies = false;
			for (Permission p : perms) {
				if(p != perm && p.implies(perm)) {
					implies = true;
					break;
				}
			}
			if(implies) {
				iter.remove();
			}
		}
	}
	
	public Class<? extends Permission> getType() {
		return permClass;
	}

	public Enumeration<Permission> elements() {
		return Collections.enumeration(perms);
	}
	
	public Set<Permission> getPermissions() {
		return perms;
	}

	@Override
	public String toString() {
		return "ClusterPermissionCollection [permClass=" + permClass + "]";
	}
}
