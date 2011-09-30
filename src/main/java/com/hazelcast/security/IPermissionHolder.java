package com.hazelcast.security;

import java.security.Permission;
import java.util.Map;

import com.hazelcast.security.permission.ClusterPermissionCollection;

public interface IPermissionHolder {

	Map<Class<? extends Permission>, ClusterPermissionCollection> getPermissions() ;
	
	ClusterPermissionCollection getPermissions(Class<? extends Permission> type);

}
