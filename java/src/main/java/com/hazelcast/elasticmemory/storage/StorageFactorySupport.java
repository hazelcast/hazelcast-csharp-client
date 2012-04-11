package com.hazelcast.elasticmemory.storage;

import com.hazelcast.elasticmemory.util.MathUtil;
import com.hazelcast.elasticmemory.util.MemorySize;
import com.hazelcast.elasticmemory.util.MemoryUnit;

import java.lang.management.ManagementFactory;
import java.lang.management.RuntimeMXBean;
import java.util.List;
import java.util.logging.Level;

public abstract class StorageFactorySupport implements StorageFactory {

    static final String MAX_HEAP_MEMORY_PARAM = "-Xmx";
    static final String MAX_DIRECT_MEMORY_PARAM = "-XX:MaxDirectMemorySize";

    static Storage createStorage(final String total, final String chunk) {
        final MemorySize totalSize = MemorySize.parse(total, MemoryUnit.MEGABYTES);
        logger.log(Level.INFO, "Elastic-Memory off-heap storage total size: " + totalSize.megaBytes() + " MB");
        final MemorySize chunkSize = MemorySize.parse(chunk, MemoryUnit.KILOBYTES);
        logger.log(Level.INFO, "Elastic-Memory off-heap storage chunk size: " + chunkSize.kiloBytes() + " KB");

        MemorySize jvmSize = getJvmDirectMemorySize();
        if (jvmSize == null) {
            jvmSize = getJvmHeapMemorySize();
        }
        if (jvmSize == null) {
            logger.log(Level.WARNING, "Either JVM max memory argument (" + MAX_HEAP_MEMORY_PARAM + ") or" +
                    " max direct memory size argument (" +
                    MAX_DIRECT_MEMORY_PARAM + ") should be configured in order to use " +
                    "Hazelcast Elastic Memory! " +
                    "(Ex: java " + MAX_DIRECT_MEMORY_PARAM + "=1G -Xmx1G -cp ...)" +
                    " Using default parameters...");
            jvmSize = new MemorySize(64, MemoryUnit.MEGABYTES);
        }
        checkOffHeapParams(jvmSize, totalSize, chunkSize);

        return new OffHeapStorage((int) totalSize.megaBytes(), (int) chunkSize.kiloBytes());
    }

    static void checkOffHeapParams(MemorySize jvm, MemorySize total, MemorySize chunk) {
        if (jvm.megaBytes() == 0) {
            throw new IllegalArgumentException(MAX_DIRECT_MEMORY_PARAM + " must be multitude of megabytes! (Current: "
                    + jvm.bytes() + " bytes)");
        }
        if (total.megaBytes() == 0) {
            throw new IllegalArgumentException("Elastic Memory total size must be multitude of megabytes! (Current: "
                    + total.bytes() + " bytes)");
        }
        if (total.megaBytes() >= jvm.megaBytes()) {
            throw new IllegalArgumentException(MAX_DIRECT_MEMORY_PARAM + " or " + MAX_HEAP_MEMORY_PARAM
                    + " must be greater than Elastic Memory total size => "
                    + MAX_DIRECT_MEMORY_PARAM + "(" + MAX_HEAP_MEMORY_PARAM + "): " + jvm.megaBytes()
                    + " megabytes, Total: " + total.megaBytes() + " megabytes");
        }
        if (chunk.kiloBytes() == 0) {
            throw new IllegalArgumentException("Elastic Memory chunk size must be multitude of kilobytes! (Current: "
                    + chunk.bytes() + " bytes)");
        }
        if (total.bytes() <= chunk.bytes()) {
            throw new IllegalArgumentException("Elastic Memory total size must be greater than chunk size => "
                    + "Total: " + total.bytes() + " bytes, Chunk: " + chunk.bytes() + " bytes");
        }
        if (!MathUtil.isPowerOf2(chunk.kiloBytes())) {
            throw new IllegalArgumentException("Elastic Memory chunk size must be power of 2 in kilobytes! (Current: "
                    + chunk.kiloBytes() + " kilobytes)");
        }
    }

    static MemorySize getJvmDirectMemorySize() {
        RuntimeMXBean rmx = ManagementFactory.getRuntimeMXBean();
        List<String> args = rmx.getInputArguments();
        for (String arg : args) {
            if (arg.startsWith(MAX_DIRECT_MEMORY_PARAM)) {
                logger.log(Level.FINEST, "Read JVM " + MAX_DIRECT_MEMORY_PARAM + " as: " + arg);
                String[] tmp = arg.split("\\=");
                if (tmp.length == 2) {
                    final String value = tmp[1];
                    return MemorySize.parse(value);
                }
                break;
            }
        }
        return null;
    }

    static MemorySize getJvmHeapMemorySize() {
        RuntimeMXBean rmx = ManagementFactory.getRuntimeMXBean();
        List<String> args = rmx.getInputArguments();
        for (String arg : args) {
            if (arg.startsWith(MAX_HEAP_MEMORY_PARAM)) {
                logger.log(Level.FINEST, "Read JVM " + MAX_HEAP_MEMORY_PARAM + " as: " + arg);
                final String value = arg.substring(MAX_HEAP_MEMORY_PARAM.length());
                return MemorySize.parse(value);
            }
        }
        return null;
    }
}
