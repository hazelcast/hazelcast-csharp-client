package com.hazelcast.elasticmemory;

import com.hazelcast.config.Config;
import com.hazelcast.core.Hazelcast;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.IMap;
import com.hazelcast.elasticmemory.error.OffHeapOutOfMemoryError;
import com.hazelcast.elasticmemory.storage.EntryRef;
import com.hazelcast.elasticmemory.storage.OffHeapStorage;
import com.hazelcast.elasticmemory.storage.Storage;
import com.hazelcast.elasticmemory.util.MemorySize;
import com.hazelcast.elasticmemory.util.MemoryUnit;
import com.hazelcast.impl.GroupProperties;
import com.hazelcast.nio.Data;
import org.junit.After;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.junit.runner.RunWith;

import java.util.Random;

import static org.junit.Assert.*;

@RunWith(com.hazelcast.util.RandomBlockJUnit4ClassRunner.class)
public class OffHeapStorageTest {

    @Before
    @After
    public void cleanup() {
        Hazelcast.shutdownAll();
    }

    @Test
    public void testPutGetRemove() {
        final int chunkSize = 2;
        final Storage s = new OffHeapStorage(32, chunkSize);
        final Random rand = new Random();
        final int k = 3072;

        byte[] data = new byte[k];
        rand.nextBytes(data);
        final int hash = rand.nextInt();

        final EntryRef ref = s.put(hash, new Data(data));
        assertEquals(k, ref.length);
        assertEquals((int) Math.ceil((double) k / (chunkSize * 1024)), ref.getChunkCount());

        Data resultData = s.get(hash, ref);
        assertNotNull(resultData);
        byte[] result = resultData.buffer;
        assertArrayEquals(data, result);

        s.remove(hash, ref);
        assertNull(s.get(hash, ref));
    }

    final MemorySize total = new MemorySize(32, MemoryUnit.MEGABYTES);
    final MemorySize chunk = new MemorySize(1, MemoryUnit.KILOBYTES);

    @Test
    public void testFillUpBuffer() {
        final int count = (int) (total.kiloBytes() / chunk.kiloBytes());
        fillUpBuffer(count);
    }

    @Test(expected = OffHeapOutOfMemoryError.class)
    public void testBufferOverFlow() {
        final int count = (int) (total.kiloBytes() / chunk.kiloBytes());
        fillUpBuffer(count + 1);
    }

    private void fillUpBuffer(int count) {
        final Storage s = new OffHeapStorage((int) total.megaBytes(), 2, (int) chunk.kiloBytes());
        byte[] data = new byte[(int) chunk.bytes()];
        for (int i = 0; i < count; i++) {
            s.put(i, new Data(data));
        }
    }

    @Test
    public void testMapStorageFull() {
        Config c = new Config();
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE, "1M");
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE, "1K");
        HazelcastInstance hz = Hazelcast.newHazelcastInstance(c);

        IMap map = hz.getMap("test");
        final byte[] value = new byte[1000];
        for (int i = 0; i < 1024; i++) {
            map.put(i, value);
        }

        map.clear();
        for (int i = 0; i < 1024; i++) {
            map.put(i, value);
        }
    }

    @Ignore
    @Test(expected = IllegalStateException.class)
    public void testMapStorageOom() {
        Config c = new Config();
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE, "1M");
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE, "1K");
        HazelcastInstance hz = Hazelcast.newHazelcastInstance(c);

        IMap map = hz.getMap("test");
        final byte[] value = new byte[1000];
        for (int i = 0; i < 1024; i++) {
            map.put(i, value);
        }
        map.put(-1, value);
    }

    @Test
    public void testMapStorageAfterDestroy() {
        Config c = new Config();
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE, "1M");
        c.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE, "1K");
        HazelcastInstance hz = Hazelcast.newHazelcastInstance(c);

        final byte[] value = new byte[1000];

        IMap map = hz.getMap("test");
        for (int i = 0; i < 1024; i++) {
            map.put(i, value);
        }
        map.destroy();

        IMap map2 = hz.getMap("test2");
        for (int i = 0; i < 1024; i++) {
            map2.put(i, value);
        }
    }

    @Ignore
    @Test(expected = IllegalStateException.class)
    public void testSharedMapStorageOom() {
        Config c1 = new Config();
        c1.getGroupConfig().setName("dev1");
        c1.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        c1.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_SHARED_STORAGE, "true");

        Config c2 = new Config();
        c2.getGroupConfig().setName("dev2");
        c2.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        c2.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_SHARED_STORAGE, "true");

        try {
            System.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE, "1M");
            System.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE, "1K");

            HazelcastInstance hz = Hazelcast.newHazelcastInstance(c1);
            HazelcastInstance hz2 = Hazelcast.newHazelcastInstance(c2);
            final byte[] value = new byte[1000];

            IMap map = hz.getMap("test");
            for (int i = 0; i < 1024; i++) {
                map.put(i, value);
            }

            hz2.getMap("test").put(1, value);

        } finally {
            System.clearProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE);
            System.clearProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE);
            Hazelcast.shutdownAll();
        }
    }

    @Ignore
    @Test
    public void testSharedMapStorageAfterShutdown() {
        Config c1 = new Config();
        c1.getGroupConfig().setName("dev1");
        c1.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        c1.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_SHARED_STORAGE, "true");

        Config c2 = new Config();
        c2.getGroupConfig().setName("dev2");
        c2.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        c2.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_SHARED_STORAGE, "true");

        try {
            System.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE, "1M");
            System.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE, "1K");

            HazelcastInstance hz = Hazelcast.newHazelcastInstance(c1);
            HazelcastInstance hz2 = Hazelcast.newHazelcastInstance(c2);
            final byte[] value = new byte[1000];

            IMap map = hz.getMap("test");
            for (int i = 0; i < 1024; i++) {
                map.put(i, value);
            }
            hz.getLifecycleService().shutdown();

            IMap map2 = hz2.getMap("test");
            for (int i = 0; i < 1024; i++) {
                map2.put(i, value);
            }

        } finally {
            System.clearProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE);
            System.clearProperty(GroupProperties.PROP_ELASTIC_MEMORY_CHUNK_SIZE);
            Hazelcast.shutdownAll();
        }
    }
}
