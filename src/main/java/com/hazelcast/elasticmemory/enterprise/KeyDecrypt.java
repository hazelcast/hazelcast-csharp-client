package com.hazelcast.elasticmemory.enterprise;

import java.io.IOException;
import java.io.InputStream;
import java.security.GeneralSecurityException;
import java.security.PublicKey;

import javax.crypto.Cipher;

import static com.hazelcast.org.apache.xerces.utils.Base64.*;

public class KeyDecrypt {

	public static String decrypt(String e) throws GeneralSecurityException, IOException  {
		String resource = "public";
		InputStream in = KeyDecrypt.class.getClassLoader().getResourceAsStream(resource);

		PublicKey key = KeyLoadStoreUtil.loadPublicKey(in, "RSA");
		
		Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");
		cipher.init(Cipher.DECRYPT_MODE, key);
		
		return new String(cipher.doFinal(decode(e.getBytes())), "UTF8");
	}
}
