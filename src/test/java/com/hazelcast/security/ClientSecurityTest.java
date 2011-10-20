package com.hazelcast.security;

import java.io.Serializable;
import java.security.AccessControlException;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import static org.junit.Assert.*;

import com.hazelcast.client.HazelcastClient;
import com.hazelcast.config.*;
import com.hazelcast.config.PermissionConfig.PermissionType;
import com.hazelcast.core.*;

public class ClientSecurityTest {

	@Before
    @After
	public void cleanup() {
		Hazelcast.shutdownAll();
	}
	
	@Test(expected = AccessControlException.class)
	public void testDenyAll() {
		final Config config = createConfig();
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		client.getMap("test").size();
		client.shutdown();
	}
	
	@Test
	public void testAllowAll() {
		final Config config = createConfig();
		addPermission(config, PermissionType.ALL, "", null);
		
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		client.getMap("test").size();
		client.getMap("test").put("a", "b");
		client.getQueue("Q").poll();
		client.shutdown();
	}
	
	@Test(expected = AccessControlException.class)
	public void testDenyEndpoint() {
		final Config config = createConfig();
		final PermissionConfig pc = addPermission(config, PermissionType.ALL, "", "dev");
		pc.addEndpoint("10.10.10.*");
		
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		client.getMap("test").size();
		client.shutdown();
	}
	
	@Test
	public void testMapAllPermission() {
		final Config config = createConfig();
		PermissionConfig perm = addPermission(config, PermissionType.MAP, "test", "dev");
		perm.addAction(SecurityConstants.ACTION_ALL);
		
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		IMap map = client.getMap("test");
		map.put("1", "A");
		map.get("1");
		map.lock("1");
		map.unlock("1");
		map.destroy();
		client.shutdown();
	}
	
	@Test(expected = AccessControlException.class)
	public void testMapPermissionActions() {
		final Config config = createConfig();
		addPermission(config, PermissionType.MAP, "test", "dev")
			.addAction(SecurityConstants.ACTION_PUT)
			.addAction(SecurityConstants.ACTION_GET)
			.addAction(SecurityConstants.ACTION_REMOVE);
		
		Hazelcast.newHazelcastInstance(config).getMap("test"); // create map
		HazelcastClient client = createHazelcastClient();
		IMap map = client.getMap("test");
		assertNull(map.put("1", "A"));
		assertEquals("A", map.get("1"));
		assertEquals("A", map.remove("1"));
		map.lock("1"); // throw exception
	}

	private HazelcastClient createHazelcastClient() {
		HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
		return client;
	}
	
	@Test
	public void testQueuePermission() {
		final Config config = createConfig();
		addPermission(config, PermissionType.QUEUE, "test", "dev")
			.addAction(SecurityConstants.ACTION_OFFER).addAction(SecurityConstants.ACTION_CREATE);
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		assertTrue(client.getQueue("test").offer("value"));
	}
	
	@Test(expected = AccessControlException.class)
	public void testQueuePermissionFail() {
		final Config config = createConfig();
		addPermission(config, PermissionType.QUEUE, "test", "dev");
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		client.getQueue("test").offer("value");
	}
	
	@Test
	public void testLockPermission() {
		final Config config = createConfig();
		addPermission(config, PermissionType.LOCK, "test", "dev")
			.addAction(SecurityConstants.ACTION_CREATE).addAction(SecurityConstants.ACTION_LOCK);
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		assertTrue(client.getLock("test").tryLock());
		client.getLock("test").unlock();
	}
	
	@Test(expected = AccessControlException.class)
	public void testLockPermissionFail() {
		final Config config = createConfig();
		addPermission(config, PermissionType.LOCK, "test", "dev")
			.addAction(SecurityConstants.ACTION_LOCK);
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		client.getLock("test").unlock();
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
		assertEquals(new Integer(1), client.getExecutorService("test").submit(new DummyCallable()).get());
	}
	
	@Test(expected = ExecutionException.class)
	public void testExecutorPermissionFail() throws InterruptedException, ExecutionException {
		final Config config = createConfig();
		addPermission(config, PermissionType.EXECUTOR_SERVICE, "test", "dev")
			.addAction(SecurityConstants.ACTION_CREATE);
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		client.getExecutorService("test").submit(new DummyCallable()).get();
	}
	
	@Test(expected = ExecutionException.class)
	public void testExecutorPermissionFail2() throws InterruptedException, ExecutionException {
		final Config config = createConfig();
		addPermission(config, PermissionType.EXECUTOR_SERVICE, "test", "dev")
			.addAction(SecurityConstants.ACTION_EXECUTE);
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		client.getExecutorService("test").submit(new DummyCallable()).get();
	}
	
	@Test(expected = ExecutionException.class)
	public void testExecutorPermissionFail3() throws InterruptedException, ExecutionException {
		final Config config = createConfig();
		addPermission(config, PermissionType.EXECUTOR_SERVICE, "test", "dev")
			.addAction(SecurityConstants.ACTION_CREATE).addAction(SecurityConstants.ACTION_EXECUTE);
		
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = createHazelcastClient();
		client.getExecutorService("test").submit(new DummyCallable()).get();
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
