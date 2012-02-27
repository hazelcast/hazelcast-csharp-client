package com.hazelcast.elasticmemory.examples;

import com.hazelcast.config.Config;
import com.hazelcast.config.XmlConfigBuilder;
import com.hazelcast.core.Hazelcast;
import com.hazelcast.core.IMap;
import com.hazelcast.core.Member;
import com.hazelcast.impl.GroupProperties;
import com.hazelcast.logging.ILogger;
import com.hazelcast.monitor.LocalMapOperationStats;
import com.hazelcast.partition.Partition;

import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.logging.Level;

public class SimpleMapTest {

    public static final int STATS_SECONDS = 10;
    public static int THREAD_COUNT = 40;
    public static int ENTRY_COUNT = 10 * 1000;
    public static int VALUE_SIZE = 1000;
    public static int GET_PERCENTAGE = 40;
    public static int PUT_PERCENTAGE = 40;
    public static int REMOVE_PERCENTAGE = 20;

    public static void main(String[] args) {
        System.setProperty("hazelcast.local.localAddress", "127.0.0.1");
        Config config = new XmlConfigBuilder().build();
        config.setLicenseKey("HGKBMJFI9CND0U22DV20H201R407Z");
        config.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_ENABLED, "true");
        config.setProperty(GroupProperties.PROP_ELASTIC_MEMORY_TOTAL_SIZE, "256m");
        Hazelcast.init(config);

        final ILogger logger = Hazelcast.getLoggingService().getLogger("SimpleMapTest");
        boolean load = false;
        boolean rm = false;
        if (args != null && args.length > 0) {
            for (String arg : args) {
                arg = arg.trim();
                if (arg.startsWith("t")) {
                    THREAD_COUNT = Integer.parseInt(arg.substring(1));
                } else if (arg.startsWith("c")) {
                    ENTRY_COUNT = Integer.parseInt(arg.substring(1));
                } else if (arg.startsWith("v")) {
                    VALUE_SIZE = Integer.parseInt(arg.substring(1));
                } else if (arg.startsWith("g")) {
                    GET_PERCENTAGE = Integer.parseInt(arg.substring(1));
                } else if (arg.startsWith("p")) {
                    PUT_PERCENTAGE = Integer.parseInt(arg.substring(1));
                } else if (arg.startsWith("r")) {
                    REMOVE_PERCENTAGE = Integer.parseInt(arg.substring(1));
                    rm = true;
                } else if (arg.startsWith("load")) {
                    load = true;
                }
            }
        } else {
            logger.log(Level.INFO, "Help: sh test.sh t200 v130 p10 g85 r20");
            logger.log(Level.INFO, "    // means 200 threads, value-size 130 bytes, 10% put, 85% get");
            logger.log(Level.INFO, "");
        }

        if (!rm) {
            REMOVE_PERCENTAGE = (100 - PUT_PERCENTAGE - GET_PERCENTAGE);
        }

        logger.log(Level.INFO, "Starting Test with ");
        logger.log(Level.INFO, "      Thread Count: " + THREAD_COUNT);
        logger.log(Level.INFO, "       Entry Count: " + ENTRY_COUNT);
        logger.log(Level.INFO, "        Value Size: " + VALUE_SIZE);
        logger.log(Level.INFO, "    Get Percentage: " + GET_PERCENTAGE);
        logger.log(Level.INFO, "    Put Percentage: " + PUT_PERCENTAGE);
        logger.log(Level.INFO, " Remove Percentage: " + REMOVE_PERCENTAGE);
        ExecutorService es = Executors.newFixedThreadPool(THREAD_COUNT);
        final IMap<Integer, byte[]> map = Hazelcast.getMap("default");
        if (load) {
            final Member thisMember = Hazelcast.getCluster().getLocalMember();
            for (int i = 0; i < ENTRY_COUNT; i++) {
                final int key = i;
                Partition partition = Hazelcast.getPartitionService().getPartition(key);
                if (thisMember.equals(partition.getOwner())) {
                    es.execute(new Runnable() {
                        public void run() {
                            map.put(key, new byte[VALUE_SIZE]);
                        }
                    });
                }
            }
        }
        if (PUT_PERCENTAGE != 0 || REMOVE_PERCENTAGE != 0 || GET_PERCENTAGE != 0) {
            for (int i = 0; i < THREAD_COUNT; i++) {
                es.execute(new Runnable() {
                    public void run() {
                        while (true) {
                            int key = (int) (Math.random() * ENTRY_COUNT);
                            int operation = ((int) (Math.random() * 100));
                            if (operation < GET_PERCENTAGE) {
                                map.get(key);
                            } else if (operation < GET_PERCENTAGE + PUT_PERCENTAGE) {
                                map.put(key, new byte[VALUE_SIZE]);
                            } else {
                                map.remove(key);
                            }
                        }
                    }
                });
            }
        }
        Executors.newSingleThreadExecutor().execute(new Runnable() {
            public void run() {
                while (true) {
                    try {
                        Thread.sleep(STATS_SECONDS * 1000);
                        logger.log(Level.INFO, "cluster size:" + Hazelcast.getCluster().getMembers().size());
                        LocalMapOperationStats mapOpStats = map.getLocalMapStats().getOperationStats();
                        long period = ((mapOpStats.getPeriodEnd() - mapOpStats.getPeriodStart()) / 1000);
                        if (period == 0) {
                            continue;
                        }
                        logger.log(Level.INFO, mapOpStats.toString());
                        logger.log(Level.INFO, "Operations per Second : " + mapOpStats.total() / period);
                    } catch (InterruptedException ignored) {
                        return;
                    }
                }
            }
        });
    }
}
