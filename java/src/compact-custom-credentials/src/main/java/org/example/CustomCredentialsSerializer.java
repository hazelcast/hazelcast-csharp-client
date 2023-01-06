package org.example;

import com.hazelcast.nio.serialization.compact.CompactReader;
import com.hazelcast.nio.serialization.compact.CompactSerializer;
import com.hazelcast.nio.serialization.compact.CompactWriter;

public class CustomCredentialsSerializer implements CompactSerializer<CustomCredentials> {
    @Override
    public CustomCredentials read(CompactReader reader) {
        String username = reader.readString("username");
        String key1 = reader.readString("key1");
        String key2 = reader.readString("key2");
        return new CustomCredentials(username, key1, key2);
    }

    @Override
    public void write(CompactWriter writer, CustomCredentials object) {
        writer.writeString("username", object.getName());
        writer.writeString("key1", object.getKey1());
        writer.writeString("key2", object.getKey2());
    }

    @Override
    public String getTypeName() {
        return "custom";
    }

    @Override
    public Class<CustomCredentials> getCompactClass() {
        return CustomCredentials.class;
    }
}