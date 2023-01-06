package org.example;

import com.hazelcast.security.Credentials;

public class CustomCredentials implements Credentials {

    private String username;
    private String key1;
    private String key2;

    public CustomCredentials() {

    }

    public CustomCredentials(String username, String key1, String key2) {
        this.username = username;
        this.key1 = key1;
        this.key2 = key2;
    }

    @Override
    public String getName() {
        return username;
    }

    public String getKey1() {
        return key1;
    }

    public String getKey2() {
        return key2;
    }
}
