package com.hazelcast.security.permission;

import java.security.Permission;

public final class AllPermissions extends ClusterPermission {
	
	public AllPermissions() {
		super("<all permissions>");
	}

	public boolean implies(Permission permission) {
		return true;
	}

	@Override
	public String getActions() {
		return "<all actions>";
	}

}
