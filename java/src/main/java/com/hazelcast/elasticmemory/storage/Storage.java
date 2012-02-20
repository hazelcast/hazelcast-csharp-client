package com.hazelcast.elasticmemory.storage;


import com.hazelcast.nio.Data;

public interface Storage {

    public final static int _1K = 1024;
    public final static int _1M = _1K * _1K;

    EntryRef put(int hash, Data data);

    OffHeapData get(int hash, EntryRef entry);

    void remove(int hash, EntryRef entry);

    void destroy();
}
