package com.hazelcast.enterprise;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.util.Date;

public class RegistrationService {
	
	private static final String LICENSE_FILE = "hazelcast.license";

	public static Registration getRegistration() throws Exception {
		final byte[] data = readLicense();
		if(data == null || data.length == 0) {
			throw new FileNotFoundException("License file could not be loaded!");
		}
		final String license = KeyDecrypt.decrypt(new String(data));
		final String parts[] = license.split("\\$");
		if(parts.length > 4) {
			final LicenseType type = LicenseType.valueOf(parts[4]);
			if(type != LicenseType.ENTERPRISE) {
				throw new InvalidLicenseError();
			}
			final String name = parts[0];
			final String mode = parts[1];
			final long end = Long.parseLong(parts[2]);
			final long start = Long.parseLong(parts[3]);
			return new Registration(name, new Date(start), new Date(end), mode);
		}
		throw new InvalidLicenseError();
	}
	
	private static byte[] readLicense() throws IOException {
		ClassLoader cl = Thread.currentThread().getContextClassLoader();
		if(cl == null) {
			cl = RegistrationService.class.getClassLoader();
		}
		final InputStream in;
		if(cl != null) {
			in = cl.getResourceAsStream(LICENSE_FILE);
		} else {
			in = ClassLoader.getSystemResourceAsStream(LICENSE_FILE);
		}
		
		if(in == null) {
			return new byte[0];
		}
		try {
			byte[] buffer = new byte[256];
			int k = 0;
			int i = 0;
			while((k = in.read()) > -1) {
				if(buffer.length == i) {
					int newSize = (int)(i * 2);
					byte[] tmp = new byte[newSize];
					System.arraycopy(buffer, 0, tmp, 0, i);
					buffer = tmp;
				}
				buffer[i++] = (byte)k;
			}
			
			byte[] out = new byte[i];
			System.arraycopy(buffer, 0, out, 0, i);
			return out;
		} finally {
			in.close();
		}
	}
	
}
