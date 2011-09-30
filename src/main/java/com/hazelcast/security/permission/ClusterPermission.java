package com.hazelcast.security.permission;

import static com.hazelcast.security.SecurityConstants.*;

import java.security.Permission;

public abstract class ClusterPermission extends Permission {
	
	protected final static int NONE = 0x0;
	
	private int hashcode = 0;
	private final int mask;
	private final String actions;

	public ClusterPermission(String name) {
		this(name, new String[0]);
	}
	
	public ClusterPermission(String name, String... actions) {
		super(name);
		if(name == null || "".equals(name)) {
			throw new IllegalArgumentException("Permission name is mamdatory!");
		}
		mask = initMask(actions);
		
		final StringBuilder s = new StringBuilder();
		for (int i = 0; i < actions.length; i++) {
			s.append(actions[i]).append(" ");
		}
		this.actions = s.toString();
	}
	
	/**
	 * init mask
	 */
	protected abstract int initMask(String[] actions); 

	public boolean implies(Permission permission) {
		if(this.getClass() != permission.getClass()) {
			return false;
		}
		
		ClusterPermission that = (ClusterPermission) permission;
		
		boolean maskTest = ((this.mask & that.mask) == that.mask);
		if(!maskTest) {
			return false;
		}
		
		if(!nameMatches(that.getName(), this.getName())) {
			return false;
		}
		
		return true;
	}
	
	public String getActions() {
		return actions;
	}
	
	public int hashCode() {
		if(hashcode == 0) {
			final int prime = 31;
			int result = 1;
			result = prime * result + (getName() != null 
					? getName().hashCode() : 13);
			hashcode = result;
		}
		return hashcode;
	}

	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		ClusterPermission other = (ClusterPermission) obj;
		if(getName() == null && other.getName() != null){
			return false;
		}
		if(!getName().equals(other.getName())) {
			return false;
		}
		if(mask != other.mask) {
			return false;
		}
		return true;
	}
}
