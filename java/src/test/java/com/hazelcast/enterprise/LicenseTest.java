package com.hazelcast.enterprise;

import com.hazelcast.config.Config;
import com.hazelcast.config.InMemoryXmlConfig;
import org.junit.Assert;
import org.junit.Test;

import java.util.HashSet;
import java.util.Random;
import java.util.Set;

public class LicenseTest {

    @Test
    public void testXmlConfig() {
        String xml = "<hazelcast>\n" +
                "    <group>\n" +
                "        <name>dev</name>\n" +
                "        <password>dev-pass</password>\n" +
                "    </group>\n" +
                "    <license-key>IEL9GHFPOMCAN20V280250I6U10N02</license-key>\n" +
                "    <network>\n" +
                "        <port auto-increment=\"true\">5701</port>\n" +
                "        <join>\n" +
                "            <multicast enabled=\"true\">\n" +
                "                <multicast-group>224.2.2.3</multicast-group>\n" +
                "                <multicast-port>54327</multicast-port>\n" +
                "            </multicast>\n" +
                "            <tcp-ip enabled=\"false\">\n" +
                "                <interface>127.0.0.1</interface>\n" +
                "            </tcp-ip>\n" +
                "        </join>\n" +
                "        <interfaces enabled=\"false\">\n" +
                "            <interface>10.10.1.*</interface>\n" +
                "        </interfaces>\n" +
                "    </network>" +
                "</hazelcast>";

        Config config = new InMemoryXmlConfig(xml);
        Assert.assertEquals("IEL9GHFPOMCAN20V280250I6U10N02", config.getLicenseKey());
    }

    @Test
    public void testKeygen() {
        int count = 100000;
        Random rand = new Random();
        for (int i = 0; i < count; i++) {
            boolean full = rand.nextBoolean();
            boolean enterprise = rand.nextBoolean();
            int day = rand.nextInt(30) + 1;
            int month = rand.nextInt(12);
            int year = KeyGenUtil.yearBase + rand.nextInt(10);
            int nodes = rand.nextInt(9999);
            License expected = new License(full, enterprise, day, month, year, nodes);
            char[] key = KeyGenUtil.generateKey(full, enterprise, day, month, year, nodes);
            License license = KeyGenUtil.extractLicense(key);
            Assert.assertEquals(expected, license);
        }
    }

    @Test
    public void testKeygenUniqueness() {
        int count = 500000;
        Set<String> keys = new HashSet<String>(count);
        boolean full = true;
        boolean enterprise = true;
        int day = 13;
        int month = 11;
        int year = 2012;
        int nodes = 9876;
        for (int i = 0; i < count; i++) {
            char[] key = KeyGenUtil.generateKey(full, enterprise, day, month, year, nodes);
            Assert.assertTrue(keys.add(new String(key)));
        }
    }
}
