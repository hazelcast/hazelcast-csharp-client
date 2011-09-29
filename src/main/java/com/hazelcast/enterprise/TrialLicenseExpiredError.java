package com.hazelcast.enterprise;

public class TrialLicenseExpiredError extends Error {

	public TrialLicenseExpiredError() {
		super("Trial license has been expired! Please contact sales@hazelcast.com");
	}
}
