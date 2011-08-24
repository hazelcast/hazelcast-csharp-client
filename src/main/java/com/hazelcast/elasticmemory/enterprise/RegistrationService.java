package com.hazelcast.elasticmemory.enterprise;

import java.io.IOException;
import java.io.InputStream;
import java.util.Date;

import com.hazelcast.impl.base.NodeInitializer;

public class RegistrationService {

	public static Registration getRegistration() throws Exception {
		final String license = KeyDecrypt.decrypt(new String(readLicense()));
		final String parts[] = license.split("\\$");
		final String name = parts[0];
		final String type = parts[1];
		final long end = Long.parseLong(parts[2]);
		final long start = Long.parseLong(parts[3]);
		return new Registration(name, new Date(start), new Date(end), type);
	}
	
	private static byte[] readLicense() throws IOException {
		final InputStream in = NodeInitializer.class.getClassLoader()
			.getResourceAsStream("hazelcast.license");
		if(in == null) {
			return new byte[0];
		}
		try {
			byte[] buffer = new byte[1024];
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
