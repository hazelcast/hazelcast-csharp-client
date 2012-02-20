package com.hazelcast.elasticmemory.storage;

import com.hazelcast.cluster.NodeAware;
import com.hazelcast.impl.Node;
import com.hazelcast.nio.Data;

import java.util.logging.Level;

import static com.hazelcast.elasticmemory.util.MathUtil.divideByAndCeil;


public class OffHeapStorage extends OffHeapStorageSupport implements Storage, NodeAware {

    private final BufferSegment[] segments;
    private Node node;

    public OffHeapStorage(int totalSizeInMb, int chunkSizeInKb) {
        this(totalSizeInMb, divideByAndCeil(totalSizeInMb, MAX_SEGMENT_SIZE_IN_MB), chunkSizeInKb);
    }

    public OffHeapStorage(int totalSizeInMb, int segmentCount, int chunkSizeInKb) {
        super(totalSizeInMb, segmentCount, chunkSizeInKb);
        logger.log(Level.INFO, "Total of " + segmentCount + " segments is going to be initialized...");
        this.segments = new BufferSegment[segmentCount];
        for (int i = 0; i < segmentCount; i++) {
            segments[i] = new BufferSegment(segmentSizeInMb, chunkSizeInKb);
        }
    }

    private BufferSegment getSegment(int hash) {
        return segments[(hash == Integer.MIN_VALUE) ? 0 : Math.abs(hash) % segmentCount];
    }

    public EntryRef put(int hash, Data value) {
        return getSegment(hash).put(value);
    }

    public OffHeapData get(int hash, EntryRef entry) {
        return getSegment(hash).get(entry);
    }

    public void remove(int hash, EntryRef entry) {
        getSegment(hash).remove(entry);
    }

    public void destroy() {
        destroy(segments);
    }

    public Node getNode() {
        return node;
    }

    public void setNode(final Node node) {
        this.node = node;
        for (int i = 0; i < segmentCount; i++) {
            segments[i].setNode(node);
        }
    }
}
