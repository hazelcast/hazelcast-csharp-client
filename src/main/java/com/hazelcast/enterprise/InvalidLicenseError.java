package com.hazelcast.enterprise;

public class InvalidLicenseError extends Error {

	public InvalidLicenseError() {
		super("Invalid license file! Please contact sales@hazelcast.com");
	}

}
