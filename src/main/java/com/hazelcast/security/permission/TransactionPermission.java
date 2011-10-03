package com.hazelcast.security.permission;

import java.security.Permission;

public class TransactionPermission extends ClusterPermission {
	
	public TransactionPermission() {
		super("Transaction");
	}
	
	public boolean implies(Permission permission) {
		if(this.getClass() != permission.getClass()) {
			return false;
		}
		return true;
	}

	public String getActions() {
		return "transaction";
	}

}
