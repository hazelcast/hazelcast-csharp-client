package com.hazelcast.security;

import java.security.Permission;
import java.security.Principal;
import java.util.Map;

import com.hazelcast.security.permission.ClusterPermissionCollection;

public interface IPermissionHolder extends Principal {

	Map<Class<? extends Permission>, ClusterPermissionCollection> getPermissions() ;
	
	ClusterPermissionCollection getPermissions(Class<? extends Permission> type);

}
