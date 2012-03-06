package com.hazelcast.enterprise;

public class InvalidLicenseError extends Error {

	public InvalidLicenseError() {
		super("Invalid license key! Please contact sales@hazelcast.com");
	}

	public InvalidLicenseError(String message) {
		super(message);
	}
	
}
