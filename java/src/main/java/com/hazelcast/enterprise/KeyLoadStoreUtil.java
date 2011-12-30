package com.hazelcast.enterprise;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.security.KeyFactory;
import java.security.NoSuchAlgorithmException;
import java.security.PrivateKey;
import java.security.PublicKey;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.PKCS8EncodedKeySpec;
import java.security.spec.X509EncodedKeySpec;

public class KeyLoadStoreUtil {
	
	public static void storePublicKey(OutputStream out, PublicKey key) throws IOException {
		X509EncodedKeySpec x509EncodedKeySpec = new X509EncodedKeySpec(key.getEncoded());
		out.write(x509EncodedKeySpec.getEncoded());
	}

	public static void storePrivateKey(OutputStream out, PrivateKey key) throws IOException {
		PKCS8EncodedKeySpec pkcs8EncodedKeySpec = new PKCS8EncodedKeySpec(key.getEncoded());
		out.write(pkcs8EncodedKeySpec.getEncoded());
	}
	
	public static PublicKey loadPublicKey(InputStream in, String algorithm) 
		throws IOException, NoSuchAlgorithmException, InvalidKeySpecException {
		
		byte[] encodedPublicKey = read(in);
		KeyFactory keyFactory = KeyFactory.getInstance(algorithm);
		X509EncodedKeySpec publicKeySpec = new X509EncodedKeySpec(encodedPublicKey);
		PublicKey publicKey = keyFactory.generatePublic(publicKeySpec);
		return publicKey;
	}

	public static PrivateKey loadPrivateKey(InputStream in, String algorithm) 
			throws IOException, NoSuchAlgorithmException, InvalidKeySpecException {

		byte[] encodedPrivateKey = read(in);
		KeyFactory keyFactory = KeyFactory.getInstance(algorithm);
		PKCS8EncodedKeySpec privateKeySpec = new PKCS8EncodedKeySpec(encodedPrivateKey);
		PrivateKey privateKey = keyFactory.generatePrivate(privateKeySpec);
		return privateKey;
	}

	private static byte[] read(InputStream in) throws IOException {
		if(in == null) {
			return new byte[0];
		}
		float factor = 1.5f;
		byte[] buffer = new byte[256];
		int k = 0;
		int i = 0;
		while((k = in.read()) > -1) {
			if(buffer.length == i) {
				int newSize = (int)(i * factor);
				byte[] tmp = new byte[newSize];
				System.arraycopy(buffer, 0, tmp, 0, i);
				buffer = tmp;
			}
			buffer[i++] = (byte)k;
		}
		return buffer; 
	}
}
