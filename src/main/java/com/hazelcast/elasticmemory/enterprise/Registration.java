package com.hazelcast.elasticmemory.enterprise;

import java.util.Date;

public class Registration {

	private static final long serialVersionUID = 8244902115220303228L;
	
	private String owner;
	private Date registryDate;
	private Date expiryDate;
	private Type type;
	
	public Registration(String owner, Date registryDate,
			Date expiryDate, String type) {
		super();
		
		this.owner = owner;
		this.registryDate = registryDate;
		this.expiryDate = expiryDate;
		this.type = Type.valueOf(type);
	}
	
	public String getOwner() {
		return owner;
	}
	
	public Date getRegistryDate() {
		return registryDate;
	}

	public Date getExpiryDate() {
		return expiryDate;
	}

	public Type getType() {
		return type;
	}
	
	public boolean isExpired() {
		return new Date().after(expiryDate);
	}
	
	public boolean isValid() {
		return true;
	}
	
	public enum Type {
		TRIAL,
		FULL
	}
}
