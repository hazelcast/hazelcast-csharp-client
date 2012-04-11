package com.hazelcast.enterprise;

import com.hazelcast.config.Config;
import com.hazelcast.core.Hazelcast;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.impl.GroupProperties;

/**
 * @mdogan 4/6/12
 */
public class Test {

    public static void main(String[] args) {
        Config c = new Config();
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE, "128M");
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE, "1K");
        c.setProperty(GroupProperties.PROP_ENTERPRISE_LICENSE_KEY, "MAN9PFEHDIK9361S8121W0060V0081");
        HazelcastInstance hz = Hazelcast.newHazelcastInstance(c);

        hz.getLifecycleService().shutdown();

    }

}
