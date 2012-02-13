package com.hazelcast.elasticmemory.storage;

public class EntryRef {

    public static final EntryRef EMPTY_DATA_REF = new EntryRef(null, 0);

    public final int length;
    private final int[] chunks;
    private boolean valid = true;

    EntryRef(int[] indexes, int length) {
        this.chunks = indexes;
        this.length = length;
    }

    public boolean isEmpty() {
        return getChunkCount() == 0;
    }

    public int getChunkCount() {
        return chunks != null ? chunks.length : 0;
    }

    public int getChunk(int i) {
        return chunks[i];
    }

    boolean isValid() {
        return valid;
    }

    void invalidate() {
        valid = false;
    }

    protected Object clone() throws CloneNotSupportedException {
        throw new CloneNotSupportedException();
    }
}