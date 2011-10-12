package com.hazelcast.enterprise;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.util.Date;
import java.util.logging.Level;

import com.hazelcast.logging.ILogger;

class RegistrationService {
	
	private static final String LICENSE_FILE = "hazelcast.license";

	static Registration getRegistration(final String path, ILogger logger) throws Exception {
		final byte[] data = readLicense(path, logger);
		if(data == null || data.length == 0) {
			throw new InvalidLicenseError("License file could not be loaded!");
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
	
	private static byte[] readLicense(String path, ILogger logger) throws IOException {
		InputStream in = null;
		if(path != null) {
			logger.log(Level.INFO, "Loading Hazelcast Enterprise license from: " + path);
			in = readFromUrl(path, logger);
			if(in == null) {
				in = readFromClasspath(path, logger);
			}
			if(in == null) {
				in = readFromFile(path, logger);
			}
			if(in == null) {
				logger.log(Level.WARNING, "Could not load Hazelcast Enterprise license from: " + path);
			}
		} 
		
		if(in == null) {
			logger.log(Level.INFO, "Trying to load Hazelcast Enterprise license from classpath.");
			in = readFromClasspath(LICENSE_FILE, logger);
			if(in == null) {
				logger.log(Level.INFO, "Trying to load Hazelcast Enterprise license from working directory.");
				in = readFromFile(LICENSE_FILE, logger);
			}
		}
		
		if(in == null) {
			return null;
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
			try {
				in.close();
			} catch (Exception ignored) {
			}
		}
	}
	
	private static InputStream readFromClasspath(String licensePath, ILogger logger) {
		ClassLoader cl = Thread.currentThread().getContextClassLoader();
		if(cl == null) {
			cl = RegistrationService.class.getClassLoader();
		}
		final InputStream in;
		if(cl != null) {
			in = cl.getResourceAsStream(licensePath);
		} else {
			in = ClassLoader.getSystemResourceAsStream(licensePath);
		}
		return in;
	}
	
	private static InputStream readFromFile(String path, ILogger logger) {
		final File file = new File(path);
		if(file.exists()) {
			try {
				return new FileInputStream(file);
			} catch (IOException e) {
				logger.log(Level.FINEST, "Could not read license file: " 
						+ path + " (" + e.getMessage() + ").");
			}
		}
		return null;
	}
	
	private static InputStream readFromUrl(String path, ILogger logger) {
		try {
			final URL url = new URL(path);
			return url.openStream();
		}
		catch(IOException e) {
			logger.log(Level.FINEST, "Could not read license file: " 
					+ path + " (" + e.getMessage() + ").");
		}
		return null;
	}
	
}
