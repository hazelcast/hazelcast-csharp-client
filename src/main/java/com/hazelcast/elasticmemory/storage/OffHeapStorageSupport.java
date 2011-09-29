package com.hazelcast.elasticmemory.storage;

import static com.hazelcast.elasticmemory.util.MathUtil.*;

abstract class OffHeapStorageSupport {

	protected static final int MAX_SEGMENT_SIZE_IN_MB = 1024;
	protected static final int MIN_SEGMENT_COUNT = 2;

	protected final int segmentCount;
	protected final int segmentSizeInMb;
	
	public OffHeapStorageSupport(int totalSizeInMb, int chunkSizeInKb) {
		this(totalSizeInMb, Math.max(MIN_SEGMENT_COUNT, divideByAndCeil(totalSizeInMb, MAX_SEGMENT_SIZE_IN_MB)), chunkSizeInKb);
	}
	
	public OffHeapStorageSupport(int totalSizeInMb, int segmentCount, int chunkSizeInKb) {
		super();
		
		int segmentSizeInMb = divideByAndCeil(totalSizeInMb, segmentCount);
		if(segmentSizeInMb > MAX_SEGMENT_SIZE_IN_MB) {
			System.err.println("Segment size exceeded max value! Setting segment size to max = " + MAX_SEGMENT_SIZE_IN_MB + "MB.");
			segmentSizeInMb = MAX_SEGMENT_SIZE_IN_MB;
		}
		
		if(totalSizeInMb % segmentSizeInMb != 0) {
			totalSizeInMb = normalize(totalSizeInMb, segmentSizeInMb);
			System.err.println("Adjusting totalSize to: " + totalSizeInMb);
		}
		
		segmentCount = totalSizeInMb / segmentSizeInMb;
		if(totalSizeInMb % segmentCount != 0) {
			throw new AssertionError("Total size[" + totalSizeInMb + " MB] must be multitude of segment count[" + segmentCount + "].");
		}
		this.segmentCount = segmentCount;
		this.segmentSizeInMb = segmentSizeInMb;
	}
	
}
