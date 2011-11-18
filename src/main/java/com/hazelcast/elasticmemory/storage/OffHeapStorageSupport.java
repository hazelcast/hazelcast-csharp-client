package com.hazelcast.elasticmemory.storage;

import static com.hazelcast.elasticmemory.util.MathUtil.*;

import java.util.logging.Level;

import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;

public abstract class OffHeapStorageSupport {
	
	protected static final int MAX_SEGMENT_SIZE_IN_MB = 1024;

	protected final ILogger logger = Logger.getLogger(getClass().getName());
	protected final int segmentCount;
	protected final int segmentSizeInMb;
	
	public OffHeapStorageSupport(int totalSizeInMb, int segmentCount, int chunkSizeInKb) {
		super();
		
		int segmentSizeInMb = divideByAndCeil(totalSizeInMb, segmentCount);
		if(segmentSizeInMb > MAX_SEGMENT_SIZE_IN_MB) {
			logger.log(Level.WARNING, "Segment size exceeded max value! Setting segment size to max = " + MAX_SEGMENT_SIZE_IN_MB + "MB.");
			segmentSizeInMb = MAX_SEGMENT_SIZE_IN_MB;
		}
		
		if(totalSizeInMb % segmentSizeInMb != 0) {
			totalSizeInMb = normalize(totalSizeInMb, segmentSizeInMb);
			logger.log(Level.WARNING, "Adjusting totalSize to: " + totalSizeInMb);
		}
		
		segmentCount = totalSizeInMb / segmentSizeInMb;
		if(totalSizeInMb % segmentCount != 0) {
			throw new AssertionError("Total size[" + totalSizeInMb + " MB] must be multitude of segment count[" + segmentCount + "].");
		}
		this.segmentCount = segmentCount;
		this.segmentSizeInMb = segmentSizeInMb;
	}
	
	protected final void destroy(final Destroyable... destroyables) {
		for (int i = 0; i < destroyables.length; i++) {
			if(destroyables[i] != null) {
				destroyables[i].destroy();
				destroyables[i] = null;
			}
		}
		System.gc();
	}

	protected interface Destroyable {
		void destroy();
	}
}
