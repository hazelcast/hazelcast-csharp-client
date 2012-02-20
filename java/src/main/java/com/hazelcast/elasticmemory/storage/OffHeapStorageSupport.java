package com.hazelcast.elasticmemory.storage;

import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;

import java.io.Closeable;
import java.util.logging.Level;

import static com.hazelcast.elasticmemory.util.MathUtil.divideByAndCeil;
import static com.hazelcast.elasticmemory.util.MathUtil.normalize;

public abstract class OffHeapStorageSupport {

    protected static final int MAX_SEGMENT_SIZE_IN_MB = 1024;

    protected final ILogger logger = Logger.getLogger(getClass().getName());
    protected final int segmentCount;
    protected final int segmentSizeInMb;

    public OffHeapStorageSupport(int totalSizeInMb, int segmentCount, int chunkSizeInKb) {
        super();

        int segmentSizeInMb = divideByAndCeil(totalSizeInMb, segmentCount);
        if (segmentSizeInMb > MAX_SEGMENT_SIZE_IN_MB) {
            logger.log(Level.WARNING, "Segment size exceeded max value! Setting segment size to max = "
                    + MAX_SEGMENT_SIZE_IN_MB + "MB.");
            segmentSizeInMb = MAX_SEGMENT_SIZE_IN_MB;
        }

        if (totalSizeInMb % segmentSizeInMb != 0) {
            totalSizeInMb = normalize(totalSizeInMb, segmentSizeInMb);
            logger.log(Level.WARNING, "Adjusting totalSize to: " + totalSizeInMb);
        }

        segmentCount = totalSizeInMb / segmentSizeInMb;
        if (totalSizeInMb % segmentCount != 0) {
            throw new AssertionError("Total size[" + totalSizeInMb + " MB] must be multitude of segment count["
                    + segmentCount + "].");
        }
        this.segmentCount = segmentCount;
        this.segmentSizeInMb = segmentSizeInMb;
    }

    protected final void destroy(final Closeable... resources) {
        for (int i = 0; i < resources.length; i++) {
            final Closeable resource;
            if ((resource = resources[i]) != null) {
                try {
                    resources[i] = null;
                    resource.close();
                } catch (Throwable e) {
                    logger.log(Level.WARNING, e.getMessage(), e);
                }
            }
        }
        System.gc();
    }
}
