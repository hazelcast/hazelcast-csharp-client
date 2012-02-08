package com.hazelcast.security;

import com.hazelcast.config.Config;
import com.hazelcast.config.CredentialsFactoryConfig;
import com.hazelcast.config.GroupConfig;
import com.hazelcast.config.SecurityConfig;
import com.hazelcast.core.Hazelcast;
import com.hazelcast.core.HazelcastInstance;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;

import java.io.DataInput;
import java.io.DataOutput;
import java.io.IOException;
import java.util.Properties;

import static org.junit.Assert.assertEquals;

@RunWith(com.hazelcast.util.RandomBlockJUnit4ClassRunner.class)
public class MemberSecurityTest {

    @Before
    @After
    public void cleanup() {
        Hazelcast.shutdownAll();
    }

    @Test
    public void testAcceptMemberMulticast() {
        final Config config = new Config();
        final SecurityConfig secCfg = config.getSecurityConfig();
        secCfg.setEnabled(true);

        Hazelcast.newHazelcastInstance(config); // master
        HazelcastInstance member = Hazelcast.newHazelcastInstance(config);
        assertEquals(2, member.getCluster().getMembers().size());
    }

    @Test
    public void testAcceptMemberTcpIp() {
        final Config config = createTcpIpConfig();
        final SecurityConfig secCfg = config.getSecurityConfig();
        secCfg.setEnabled(true);

        Hazelcast.newHazelcastInstance(config); // master
        HazelcastInstance member = Hazelcast.newHazelcastInstance(config);
        assertEquals(2, member.getCluster().getMembers().size());
    }

    @Test(expected = IllegalStateException.class)
    public void testDenyMemberWrongCredentials() {
        final Config config = new Config();
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
    public void testDenyMemberMulticast() {
        final Config config = new Config();
        final SecurityConfig secCfg = config.getSecurityConfig();
        secCfg.setEnabled(true);

        Hazelcast.newHazelcastInstance(config); // master
        Hazelcast.newHazelcastInstance(new Config());
    }

    @Test(expected = IllegalStateException.class)
    public void testDenyMemberTcpIp() {
        final Config config = createTcpIpConfig();
        final SecurityConfig secCfg = config.getSecurityConfig();
        secCfg.setEnabled(true);

        Hazelcast.newHazelcastInstance(config); // master
        Hazelcast.newHazelcastInstance(createTcpIpConfig());
    }

    private Config createTcpIpConfig() {
        final Config config = new Config();
        config.getNetworkConfig().getJoin().getMulticastConfig().setEnabled(false);
        config.getNetworkConfig().getJoin().getTcpIpConfig().setEnabled(true)
                .clear().addMember("127.0.0.1");
        config.getNetworkConfig().getInterfaces().clear().addInterface("127.0.0.1");
        return config;
    }
}
