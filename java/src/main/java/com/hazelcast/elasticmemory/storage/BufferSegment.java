package com.hazelcast.elasticmemory.storage;

import com.hazelcast.cluster.NodeAware;
import com.hazelcast.elasticmemory.error.OffHeapError;
import com.hazelcast.elasticmemory.error.OffHeapOutOfMemoryError;
import com.hazelcast.impl.Node;
import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;
import com.hazelcast.nio.Data;

import java.io.Closeable;
import java.nio.ByteBuffer;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.logging.Level;

import static com.hazelcast.elasticmemory.util.MathUtil.divideByAndCeil;

public class BufferSegment implements Closeable, NodeAware {

    private static final ILogger logger = Logger.getLogger(BufferSegment.class.getName());

    public final static int _1K = 1024;
    public final static int _1M = _1K * _1K;

    private static int ID = 0;

    private static synchronized int nextId() {
        return ID++;
    }

    private final Lock lock = new ReentrantLock();
    private final int totalSize;
    private final int chunkSize;
    private final int chunkCount;
    private AddressQueue chunks;
    private volatile ByteBuffer mainBuffer; // used only for duplicates; no read, no write
    private ByteBuffer serviceThreadBuffer;
    private Node node;

    public BufferSegment(int totalSizeInMb, int chunkSizeInKb) {
        super();
        this.totalSize = totalSizeInMb * _1M;
        this.chunkSize = chunkSizeInKb * _1K;

        assertTrue((totalSize % chunkSize == 0), "Segment size[" + totalSizeInMb
                + " MB] must be multitude of chunk size[" + chunkSizeInKb + " KB]!");

        final int index = nextId();
        this.chunkCount = totalSize / chunkSize;
        logger.log(Level.INFO, "BufferSegment[" + index + "] starting with chunkCount=" + chunkCount);

        chunks = new AddressQueue(chunkCount);
        mainBuffer = ByteBuffer.allocateDirect(totalSize);
        serviceThreadBuffer = mainBuffer.duplicate();
        for (int i = 0; i < chunkCount; i++) {
            chunks.offer(i);
        }
        logger.log(Level.INFO, "BufferSegment[" + index + "] started!");
    }

    public EntryRef put(final Data data) {
        final byte[] value = data != null ? data.buffer : null;
        if (value == null || value.length == 0) {
            return EntryRef.EMPTY_DATA_REF;
        }

        final int count = divideByAndCeil(value.length, chunkSize);
        final int[] indexes = reserve(count);  // operation under lock
        final ByteBuffer buffer = getBuffer(false);   // volatile read
        if (buffer == null) {
            throw new BufferSegmentClosedError();
        }
        int offset = 0;
        for (int i = 0; i < count; i++) {
            buffer.position(indexes[i] * chunkSize);
            int len = Math.min(chunkSize, (value.length - offset));
            buffer.put(value, offset, len);
            offset += len;
        }
        return new EntryRef(indexes, value.length); // volatile write
    }

    public OffHeapData get(final EntryRef ref) {
        if (!isEntryRefValid(ref)) {  // volatile read
            return null;
        }

        final byte[] value = new byte[ref.length];
        final int chunkCount = ref.getChunkCount();
        int offset = 0;
        final ByteBuffer buffer = getBuffer(true);  // volatile read
        if (buffer == null) {
            throw new BufferSegmentClosedError();
        }
        for (int i = 0; i < chunkCount; i++) {
            buffer.position(ref.getChunk(i) * chunkSize);
            int len = Math.min(chunkSize, (ref.length - offset));
            buffer.get(value, offset, len);
            offset += len;
        }
        // volatile read
        return isEntryRefValid(ref) ? new OffHeapData(value, OffHeapData.VALID)
                : OffHeapData.INVALID_OFF_HEAP_DATA;
    }

    private ByteBuffer getBuffer(boolean readonly) { // volatile read
        return mainBuffer != null ?
                (isServiceThread() ? serviceThreadBuffer :
                        (readonly ? mainBuffer.asReadOnlyBuffer() : mainBuffer.duplicate())
                ) : null;
    }

    public void remove(final EntryRef ref) {
        if (!isEntryRefValid(ref)) { // volatile read
            return;
        }
        ref.invalidate(); // volatile write
        final int chunkCount = ref.getChunkCount();
        final int[] indexes = new int[chunkCount];
        for (int i = 0; i < chunkCount; i++) {
            indexes[i] = ref.getChunk(i);
        }
        assertTrue(release(indexes), "Could not offer released indexes! Error in queue...");
    }

    private boolean isEntryRefValid(final EntryRef ref) {
        return ref != null && !ref.isEmpty() && ref.isValid();  //isValid() volatile read
    }

    private static void assertTrue(boolean condition, String message) {
        if (!condition) {
            throw new AssertionError(message);
        }
    }

    private boolean isServiceThread() {
        return node != null && node.isServiceThread();
    }

    public void close() {
        lock.lock();
        try {
            chunks = null;
            mainBuffer = null; // volatile write
            serviceThreadBuffer = null;
        } finally {
            lock.unlock();
        }
    }

    private int[] reserve(final int count) {
        lock.lock();
        try {
            if (chunks == null) {
                throw new BufferSegmentClosedError();
            }
            final int[] indexes = new int[count];
            return chunks.poll(indexes);
        } finally {
            lock.unlock();
        }
    }

    private boolean release(final int[] indexes) {
        lock.lock();
        try {
            boolean b = true;
            for (int i = 0; i < indexes.length; i++) {
                int index = indexes[i];
                b = chunks.offer(index) && b;
            }
            return b;
        } finally {
            lock.unlock();
        }
    }

    public Node getNode() {
        return node;
    }

    public void setNode(final Node node) {
        this.node = node;
    }

    private class AddressQueue {
        final static int NULL_VALUE = -1;
        final int maxSize;
        final int[] array;
        int add = 0;
        int remove = 0;
        int size = 0;

        public AddressQueue(int maxSize) {
            this.maxSize = maxSize;
            array = new int[maxSize];
        }

        public boolean offer(int value) {
            if (size == maxSize) {
                return false;
            }
            array[add++] = value;
            size++;
            if (add == maxSize) {
                add = 0;
            }
            return true;
        }

        public int poll() {
            if (size == 0) {
                return NULL_VALUE;
            }
            final int value = array[remove];
            array[remove++] = NULL_VALUE;
            size--;
            if (remove == maxSize) {
                remove = 0;
            }
            return value;
        }

        public int[] poll(final int[] indexes) {
            final int count = indexes.length;
            if (count > size) {
                throw new OffHeapOutOfMemoryError("Segment has " + size + " available chunks. " +
                        "Data requires " + count + " chunks. Segment is full!");
            }

            for (int i = 0; i < count; i++) {
                indexes[i] = poll();
            }
            return indexes;
        }
    }

    private class BufferSegmentClosedError extends OffHeapError {
        public BufferSegmentClosedError() {
            super("BufferSegment is closed!");
        }
    }
}
