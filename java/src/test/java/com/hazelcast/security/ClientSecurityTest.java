package com.hazelcast.security;

import com.hazelcast.client.ClientConfig;
import com.hazelcast.client.HazelcastClient;
import com.hazelcast.config.Config;
import com.hazelcast.config.PermissionConfig;
import com.hazelcast.config.PermissionConfig.PermissionType;
import com.hazelcast.config.SecurityConfig;
import com.hazelcast.config.XmlConfigBuilder;
import com.hazelcast.core.Hazelcast;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.HazelcastInstanceAware;
import com.hazelcast.core.IMap;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;

import java.io.Serializable;
import java.util.concurrent.Callable;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.atomic.AtomicReference;

import static org.junit.Assert.*;

@RunWith(com.hazelcast.util.RandomBlockJUnit4ClassRunner.class)
public class ClientSecurityTest {
    
    @Before
    @After
    public void cleanup() {
        HazelcastClient.shutdownAll();
        Hazelcast.shutdownAll();
    }

    @Test(expected = RuntimeException.class)
    public void testDenyAll() {
        final Config config = createConfig();
        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getMap("test").size();
        } finally {
            client.shutdown();
        }
    }

    @Test
    public void testAllowAll() {
        final Config config = createConfig();
        addPermission(config, PermissionType.ALL, "", null);

        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getMap("test").size();
            client.getMap("test").size();
            client.getMap("test").put("a", "b");
            client.getQueue("Q").poll();
        } finally {
            client.shutdown();
        }
    }

    @Test(expected = RuntimeException.class)
    public void testDenyEndpoint() {
        final Config config = createConfig();
        final PermissionConfig pc = addPermission(config, PermissionType.ALL, "", "dev");
        pc.addEndpoint("10.10.10.*");

        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getMap("test").size();
        } finally {
            client.shutdown();
        }
    }

    @Test
    public void testMapAllPermission() {
        final Config config = createConfig();
        PermissionConfig perm = addPermission(config, PermissionType.MAP, "test", "dev");
        perm.addAction(SecurityConstants.ACTION_ALL);

        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            IMap map = client.getMap("test");
            map.put("1", "A");
            map.get("1");
            map.lock("1");
            map.unlock("1");
            map.destroy();
        } finally {
            client.shutdown();
        }
    }

    @Test(expected = RuntimeException.class)
    public void testMapPermissionActions() {
        final Config config = createConfig();
        addPermission(config, PermissionType.MAP, "test", "dev")
                .addAction(SecurityConstants.ACTION_PUT)
                .addAction(SecurityConstants.ACTION_GET)
                .addAction(SecurityConstants.ACTION_REMOVE);

        Hazelcast.newHazelcastInstance(config).getMap("test"); // create map
        HazelcastClient client = createHazelcastClient();
        try {
            IMap map = client.getMap("test");
            assertNull(map.put("1", "A"));
            assertEquals("A", map.get("1"));
            assertEquals("A", map.remove("1"));
            map.lock("1"); // throw exception
        } finally {
            client.shutdown();
        }
    }

    private HazelcastClient createHazelcastClient() {
        ClientConfig config = new ClientConfig().addAddress("127.0.0.1");
        HazelcastClient client = HazelcastClient.newHazelcastClient(config);
        return client;
    }

    @Test
    public void testQueuePermission() {
        final Config config = createConfig();
        addPermission(config, PermissionType.QUEUE, "test", "dev")
                .addAction(SecurityConstants.ACTION_OFFER).addAction(SecurityConstants.ACTION_CREATE);
        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            assertTrue(client.getQueue("test").offer("value"));
        } finally {
            client.shutdown();
        }
    }

    @Test(expected = RuntimeException.class)
    public void testQueuePermissionFail() {
        final Config config = createConfig();
        addPermission(config, PermissionType.QUEUE, "test", "dev");
        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getQueue("test").offer("value");
        } finally {
            client.shutdown();
        }
    }

    @Test
    public void testLockPermission() {
        final Config config = createConfig();
        addPermission(config, PermissionType.LOCK, "test", "dev")
                .addAction(SecurityConstants.ACTION_CREATE).addAction(SecurityConstants.ACTION_LOCK);
        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            assertTrue(client.getLock("test").tryLock());
            client.getLock("test").unlock();
        } finally {
            client.shutdown();
        }
    }

    @Test(expected = RuntimeException.class)
    public void testLockPermissionFail() {
        final Config config = createConfig();
        addPermission(config, PermissionType.LOCK, "test", "dev")
                .addAction(SecurityConstants.ACTION_LOCK);
        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getLock("test").unlock();
        } finally {
            client.getLifecycleService().shutdown();
        }
    }

    @Test(expected = RuntimeException.class)
    public void testLockPermissionFail2() {
        final Config config = createConfig();
        addPermission(config, PermissionType.LOCK, "test", "dev")
                .addAction(SecurityConstants.ACTION_CREATE);
        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getLock("test").tryLock();
        } finally {
            client.getLifecycleService().shutdown();
        }
    }

    @Test
    public void testExecutorPermission() throws InterruptedException, ExecutionException {
        final Config config = createConfig();
        addPermission(config, PermissionType.EXECUTOR_SERVICE, "test", "dev")
                .addAction(SecurityConstants.ACTION_CREATE).addAction(SecurityConstants.ACTION_EXECUTE);

        addPermission(config, PermissionType.LIST, "list", null)
                .addAction(SecurityConstants.ACTION_ADD).addAction(SecurityConstants.ACTION_CREATE)
                .addAction(SecurityConstants.ACTION_GET);

        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            assertEquals(new Integer(1), client.getExecutorService("test").submit(new DummyCallable()).get());
        } finally {
            client.shutdown();
        }
    }

    @Test(expected = ExecutionException.class)
    public void testExecutorPermissionFail() throws InterruptedException, ExecutionException {
        final Config config = createConfig();
        addPermission(config, PermissionType.EXECUTOR_SERVICE, "test", "dev")
                .addAction(SecurityConstants.ACTION_CREATE);
        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getExecutorService("test").submit(new DummyCallable()).get();
        } finally {
            client.shutdown();
        }
    }

    @Test(expected = ExecutionException.class)
    public void testExecutorPermissionFail2() throws InterruptedException, ExecutionException {
        final Config config = createConfig();
        addPermission(config, PermissionType.EXECUTOR_SERVICE, "test", "dev")
                .addAction(SecurityConstants.ACTION_EXECUTE);
        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getExecutorService("test").submit(new DummyCallable()).get();
        } finally {
            client.shutdown();
        }
    }

    @Test(expected = ExecutionException.class)
    public void testExecutorPermissionFail3() throws InterruptedException, ExecutionException {
        final Config config = createConfig();
        addPermission(config, PermissionType.EXECUTOR_SERVICE, "test", "dev")
                .addAction(SecurityConstants.ACTION_CREATE).addAction(SecurityConstants.ACTION_EXECUTE);

        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            client.getExecutorService("test").submit(new DummyCallable()).get();
        } finally {
            client.shutdown();
        }
    }

    @Test
    public void testExecutorPermissionFail4() throws InterruptedException, ExecutionException {
        final Config config = createConfig();
        addPermission(config, PermissionType.EXECUTOR_SERVICE, "test", "dev")
                .addAction(SecurityConstants.ACTION_CREATE).addAction(SecurityConstants.ACTION_EXECUTE);

        Hazelcast.newHazelcastInstance(config);
        HazelcastClient client = createHazelcastClient();
        try {
            assertNull(client.getExecutorService("test").submit(new DummyCallableNewThread()).get());
        } finally {
            client.shutdown();
        }
    }

    static class DummyCallable implements Callable<Integer>, Serializable, HazelcastInstanceAware {
        HazelcastInstance hz;

        public Integer call() throws Exception {
            hz.getList("list").add("value");
            return hz.getList("list").size();
        }

        public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
            hz = hazelcastInstance;
        }
    }

    static class DummyCallableNewThread implements Callable<Integer>, Serializable, HazelcastInstanceAware {
        HazelcastInstance hz;

        public Integer call() throws Exception {
            final CountDownLatch latch = new CountDownLatch(1);
            final AtomicReference<Integer> value = new AtomicReference<Integer>();
            new Thread() {
                public void run() {
                    try {
                        hz.getList("list").add("value");
                        value.set(hz.getList("list").size());
                    } finally {
                        latch.countDown();
                    }
                }
            }.start();
            latch.await();
            return value.get();
        }

        public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
            hz = hazelcastInstance;
        }
    }

    private Config createConfig() {
        final Config config = new XmlConfigBuilder().build();
        final SecurityConfig secCfg = config.getSecurityConfig();
        secCfg.setEnabled(true);
        return config;
    }

    private PermissionConfig addPermission(Config config, PermissionType type, String name, String principal) {
        PermissionConfig perm = new PermissionConfig(type, name, principal);
        config.getSecurityConfig().addClientPermissionConfig(perm);
        return perm;
    }
}
