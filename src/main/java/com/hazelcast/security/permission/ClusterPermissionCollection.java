package com.hazelcast.security.permission;

import java.security.Permission;
import java.security.PermissionCollection;
import java.util.Collections;
import java.util.Enumeration;
import java.util.Set;

import com.hazelcast.util.ConcurrentHashSet;

public class ClusterPermissionCollection extends PermissionCollection {
	
	final Set<Permission> perms = new ConcurrentHashSet<Permission>();

	public void add(Permission permission) {
		if(permission instanceof ClusterPermission) {
			perms.add((ClusterPermission) permission);
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

	public Enumeration<Permission> elements() {
		return Collections.enumeration(perms);
	}

}
