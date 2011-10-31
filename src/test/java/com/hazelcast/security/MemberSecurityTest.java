package com.hazelcast.security;

import static org.junit.Assert.*;

import java.io.DataInput;
import java.io.DataOutput;
import java.io.IOException;
import java.util.Properties;

import org.junit.*;

import com.hazelcast.config.*;
import com.hazelcast.core.*;

public class MemberSecurityTest {

	@Before
    @After
	public void cleanup() {
		Hazelcast.shutdownAll();
	}
	
	@Test
	public void testAcceptMember() {
		final Config config = new XmlConfigBuilder().build();
		final SecurityConfig secCfg = config.getSecurityConfig();
		secCfg.setEnabled(true);
		
		Hazelcast.newHazelcastInstance(config); // master
		HazelcastInstance member = Hazelcast.newHazelcastInstance(config);
		assertEquals(2, member.getCluster().getMembers().size());
	}
	
	@Test(expected = IllegalStateException.class)
	public void testDenyMemberWrongCredentials() {
		final Config config = new XmlConfigBuilder().build();
		final SecurityConfig secCfg = config.getSecurityConfig();
		secCfg.setEnabled(true);
		CredentialsFactoryConfig credentialsFactoryConfig = new CredentialsFactoryConfig();
		credentialsFactoryConfig.setImplementation(new ICredentialsFactory() {
			public Credentials newCredentials() {
				return new InValidCredentials();
			}
			public void destroy() {
			}
			
			public void configure(GroupConfig groupConfig, Properties properties) {
			}
		});
		secCfg.setMemberCredentialsConfig(credentialsFactoryConfig);
		
		Hazelcast.newHazelcastInstance(config); // master
		Hazelcast.newHazelcastInstance(config);
	}
	
	public static class InValidCredentials extends AbstractCredentials {
		public InValidCredentials() {
			super("invalid-group-name");
		}
		protected void writeDataInternal(DataOutput out) throws IOException {
		}

		protected void readDataInternal(DataInput in) throws IOException {
		}
	}
	
	@Test(expected = IllegalStateException.class)
	public void testDenyMemberSecurityOff() {
		final Config config = new XmlConfigBuilder().build();
		final SecurityConfig secCfg = config.getSecurityConfig();
		secCfg.setEnabled(true);
		
		Hazelcast.newHazelcastInstance(config); // master
		Hazelcast.newHazelcastInstance(null);
	}
}
