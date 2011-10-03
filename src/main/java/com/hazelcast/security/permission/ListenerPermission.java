package com.hazelcast.security.permission;

import java.security.Permission;

public class ListenerPermission extends ClusterPermission {
	
	public ListenerPermission(String name) {
		super(name);
	}

	public boolean implies(Permission permission) {
		if(this.getClass() != permission.getClass()) {
			return false;
		}
		
		InstancePermission that = (InstancePermission) permission;
		
		if(!that.getName().equals(this.getName())) {
			return false;
		}
		
		return true;
	}

	public String getActions() {
		return "listener";
	}
}
