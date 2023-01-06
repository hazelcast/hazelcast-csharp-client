package org.example;

import com.hazelcast.nio.serialization.genericrecord.GenericRecord;
import com.hazelcast.security.ClusterLoginModule;
import com.hazelcast.security.Credentials;
import com.hazelcast.security.CredentialsCallback;
import com.hazelcast.security.TokenCredentials;
import com.hazelcast.security.TokenDeserializerCallback;

import javax.security.auth.callback.Callback;
import javax.security.auth.callback.UnsupportedCallbackException;
import javax.security.auth.login.LoginException;
import java.io.IOException;
import java.util.Set;

public class CustomLoginModule extends ClusterLoginModule {
    private String name;

    @Override
    protected boolean onLogin() throws LoginException {
        logger.info("CustomLoginModule: onLogin");
        CredentialsCallback cb = new CredentialsCallback();
        TokenDeserializerCallback tdcb = new TokenDeserializerCallback();
        try {
            callbackHandler.handle(new Callback[]{cb, tdcb});
        } catch (IOException | UnsupportedCallbackException e) {
            logger.info("CustomLoginModule: no credentials");
            throw new LoginException("Problem getting credentials");
        }
        Credentials credentials = cb.getCredentials();

        String creds_name, creds_key1, creds_key2;

        if (credentials instanceof TokenCredentials) {
            logger.info("CustomLoginModule: reading credentials from TokenCredentials " + credentials.getClass());
            TokenCredentials tokenCreds = (TokenCredentials) credentials;
            Object o = tdcb.getTokenDeserializer().deserialize(tokenCreds);
            logger.info("CustomLoginModule: token contains " + o.getClass());
            if (o instanceof GenericRecord) {
                logger.info("CustomLoginModule: reading credentials from GenericRecord");
                GenericRecord record = (GenericRecord) o;
                Set<String> fieldNames = record.getFieldNames();
                logger.info("CustomLoginModule: record fields = " + fieldNames);
                if (!fieldNames.contains("username") ||
                    !fieldNames.contains("key1") ||
                    !fieldNames.contains("key2")) {
                        logger.info("CustomLoginModule: record does not contain all required fields");
                        throw new LoginException("Invalid GenericRecord credentials, missing fields");
                }
                creds_name = record.getString("username");
                creds_key1 = record.getString("key1");
                creds_key2 = record.getString("key2");
            }
            else if (o instanceof CustomCredentials)
            {
                logger.info("CustomLoginModule: reading credentials from CustomCredentials");
                credentials = (Credentials) o;
                CustomCredentials cc = (CustomCredentials) credentials; 
                creds_name = cc.getName();
                creds_key1 = cc.getKey1();
                creds_key2 = cc.getKey2();            
            }
            else
            {
                logger.info("CustomLoginModule: Invalid token credentials type " + o.getClass());
                throw new LoginException("Invalid token credentials type " + o.getClass());
            }
        }
        else if (credentials instanceof CustomCredentials) {
            logger.info("CustomLoginModule: reading credentials from CustomCredentials");
            CustomCredentials cc = (CustomCredentials) credentials; 
            creds_name = cc.getName();
            creds_key1 = cc.getKey1();
            creds_key2 = cc.getKey2();
        }
        else {
            logger.info("CustomLoginModule: invalid credentials type " + credentials.getClass());
            throw new LoginException("Invalid credentials type " + credentials.getClass());
        }

        if (creds_name != null && creds_name.equals(options.get("username")) &&
            creds_key1 != null && creds_key1.equals(options.get("key1")) &&
            creds_key2 != null && creds_key2.equals(options.get("key2"))) {
            name = creds_name;
            addRole(name);
            logger.info("CustomLoginModule: success");
            return true;
        }
        logger.info("CustomLoginModule: invalid credentials");
        throw new LoginException("Invalid credentials");
    }

    @Override
    protected String getName() {
        return name;
    }
}

