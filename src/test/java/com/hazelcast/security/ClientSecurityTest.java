package com.hazelcast.security;

import java.security.AccessControlException;

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
		final Config config = new XmlConfigBuilder().build();
		final SecurityConfig secCfg = config.getSecurityConfig();
		secCfg.setEnabled(true);
		
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
		client.getMap("test").size();
		client.shutdown();
	}
	
	@Test
	public void testAllowAll() {
		final Config config = new XmlConfigBuilder().build();
		final SecurityConfig secCfg = config.getSecurityConfig();
		secCfg.setEnabled(true);
		secCfg.addClientPermissionConfig(new PermissionConfig(PermissionType.ALL, "", null));
		
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
		client.getMap("test").size();
		client.getMap("test").put("a", "b");
		client.getQueue("Q").poll();
		client.shutdown();
	}
	
	@Test(expected = AccessControlException.class)
	public void testDenyEndpoint() {
		final Config config = new XmlConfigBuilder().build();
		final SecurityConfig secCfg = config.getSecurityConfig();
		secCfg.setEnabled(true);
		final PermissionConfig pc = new PermissionConfig(PermissionType.ALL, "", "dev");
		pc.addEndpoint("10.10.10.*");
		secCfg.addClientPermissionConfig(pc);
		
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
		client.getMap("test").size();
		client.shutdown();
	}
	
	@Test
	public void testMapAllPermission() {
		final Config config = new XmlConfigBuilder().build();
		final SecurityConfig secCfg = config.getSecurityConfig();
		secCfg.setEnabled(true);
		PermissionConfig perm = new PermissionConfig(PermissionType.MAP, "test", "dev");
		secCfg.addClientPermissionConfig(perm);
		perm.addAction(SecurityConstants.ACTION_ALL);
		
		Hazelcast.newHazelcastInstance(config);
		HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
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
		final Config config = new XmlConfigBuilder().build();
		final SecurityConfig secCfg = config.getSecurityConfig();
		secCfg.setEnabled(true);
		PermissionConfig perm = new PermissionConfig(PermissionType.MAP, "test", "dev");
		secCfg.addClientPermissionConfig(perm);
		perm.addAction(SecurityConstants.ACTION_PUT);
		perm.addAction(SecurityConstants.ACTION_GET);
		perm.addAction(SecurityConstants.ACTION_REMOVE);
		
		Hazelcast.newHazelcastInstance(config).getMap("test"); // create map
		HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
		IMap map = client.getMap("test");
		assertNull(map.put("1", "A"));
		assertEquals("A", map.get("1"));
		assertEquals("A", map.remove("1"));
		map.lock("1"); // throw exception
	}
	
}
