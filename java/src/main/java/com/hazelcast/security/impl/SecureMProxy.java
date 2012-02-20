package com.hazelcast.security.impl;

import com.hazelcast.core.EntryListener;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.MapEntry;
import com.hazelcast.impl.MProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.monitor.MapOperationsCounter;
import com.hazelcast.monitor.LocalMapStats;
import com.hazelcast.query.Expression;
import com.hazelcast.query.Predicate;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.MapPermission;

import java.util.Collection;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

final class SecureMProxy extends SecureProxySupport implements MProxy {

    private final MProxy proxy;
    private final MapPermission putPermission;
    private final MapPermission getPermission;
    private final MapPermission removePermission;
    private final MapPermission listenPermission;
    private final MapPermission lockPermission;
    private final MapPermission statsPermission;

    SecureMProxy(Node node, MProxy map) {
        super(node);
        proxy = map;
        putPermission = new MapPermission(getName(), SecurityConstants.ACTION_PUT);
        getPermission = new MapPermission(getName(), SecurityConstants.ACTION_GET);
        removePermission = new MapPermission(getName(), SecurityConstants.ACTION_REMOVE);
        listenPermission = new MapPermission(getName(), SecurityConstants.ACTION_LISTEN);
        lockPermission = new MapPermission(getName(), SecurityConstants.ACTION_LOCK);
        statsPermission = new MapPermission(getName(), SecurityConstants.ACTION_STATISTICS);
    }

    private void checkPut() {
        SecurityUtil.checkPermission(node.securityContext, putPermission);
    }

    private void checkGet() {
        SecurityUtil.checkPermission(node.securityContext, getPermission);
    }

    private void checkRemove() {
        SecurityUtil.checkPermission(node.securityContext, removePermission);
    }

    private void checkListen() {
        SecurityUtil.checkPermission(node.securityContext, listenPermission);
    }

    private void checkLock() {
        SecurityUtil.checkPermission(node.securityContext, lockPermission);
    }

    // ------ IMap

    public Object putIfAbsent(Object key, Object value) {
        checkPut();
        return (Object) proxy.putIfAbsent(key, value);
    }

    public void flush() {
        proxy.flush();
    }

    public String getName() {
        return proxy.getName();
    }

    public InstanceType getInstanceType() {
        return proxy.getInstanceType();
    }

    public Map getAll(Set keys) {
        checkGet();
        return proxy.getAll(keys);
    }

    public boolean remove(Object key, Object value) {
        checkRemove();
        return proxy.remove(key, value);
    }

    public void destroy() {
        SecurityUtil.checkPermission(node.securityContext, new MapPermission(getName(), SecurityConstants.ACTION_DESTROY));
        proxy.destroy();
    }

    public Future<Object> getAsync(Object key) {
        checkGet();
        return proxy.getAsync(key);
    }

    public Object getId() {
        return proxy.getId();
    }

    public boolean replace(Object key, Object oldValue, Object newValue) {
        checkPut();
        return proxy.replace(key, oldValue, newValue);
    }

    public Future<Object> putAsync(Object key, Object value) {
        checkPut();
        return proxy.putAsync(key, value);
    }

    public Object replace(Object key, Object value) {
        checkPut();
        return (Object) proxy.replace(key, value);
    }

    public Future<Object> removeAsync(Object key) {
        checkRemove();
        return proxy.removeAsync(key);
    }

    public Object tryRemove(Object key, long timeout, TimeUnit timeunit)
            throws TimeoutException {
        checkRemove();
        return proxy.tryRemove(key, timeout, timeunit);
    }

    public boolean tryPut(Object key, Object value, long timeout, TimeUnit timeunit) {
        checkPut();
        return proxy.tryPut(key, value, timeout, timeunit);
    }

    public int size() {
        checkGet();
        return proxy.size();
    }

    public boolean isEmpty() {
        checkGet();
        return proxy.isEmpty();
    }

    public boolean containsKey(Object key) {
        checkGet();
        return proxy.containsKey(key);
    }

    public Object put(Object key, Object value, long ttl, TimeUnit timeunit) {
        checkPut();
        return (Object) proxy.put(key, value, ttl, timeunit);
    }

    public void putTransient(Object key, Object value, long ttl, TimeUnit timeunit) {
        checkPut();
        proxy.putTransient(key, value, ttl, timeunit);
    }

    public boolean containsValue(Object value) {
        checkGet();
        return proxy.containsValue(value);
    }

    public Object putIfAbsent(Object key, Object value, long ttl, TimeUnit timeunit) {
        checkPut();
        return (Object) proxy.putIfAbsent(key, value, ttl, timeunit);
    }

    public void set(final Object key, final Object value, final long ttl, final TimeUnit timeunit) {
        checkPut();
        proxy.set(key, value, ttl, timeunit);
    }

    public Object tryLockAndGet(Object key, long time, TimeUnit timeunit)
            throws TimeoutException {
        checkGet();
        checkLock();
        return (Object) proxy.tryLockAndGet(key, time, timeunit);
    }

    public Object get(Object key) {
        checkGet();
        return (Object) proxy.get(key);
    }

    public void putAndUnlock(Object key, Object value) {
        checkPut();
        proxy.putAndUnlock(key, value);
    }

    public void lock(Object key) {
        checkLock();
        proxy.lock(key);
    }

    public Object put(Object key, Object value) {
        checkPut();
        return (Object) proxy.put(key, value);
    }

    public boolean tryLock(Object key) {
        checkLock();
        return proxy.tryLock(key);
    }

    public boolean tryLock(Object key, long time, TimeUnit timeunit) {
        checkLock();
        return proxy.tryLock(key, time, timeunit);
    }

    public void unlock(Object key) {
        proxy.unlock(key);
    }

    public void forceUnlock(final Object key) {
        checkLock();
        proxy.forceUnlock(key);
    }

    public boolean lockMap(long time, TimeUnit timeunit) {
        checkLock();
        return proxy.lockMap(time, timeunit);
    }

    public Object remove(Object key) {
        checkRemove();
        return (Object) proxy.remove(key);
    }

    public void unlockMap() {
        proxy.unlockMap();
    }

    public void addLocalEntryListener(EntryListener listener) {
        checkListen();
        proxy.addLocalEntryListener(listener);
    }

    public void putAll(Map t) {
        checkPut();
        proxy.putAll(t);
    }

    public void addEntryListener(EntryListener listener,
                                 boolean includeValue) {
        checkListen();
        proxy.addEntryListener(listener, includeValue);
    }

    public void removeEntryListener(EntryListener listener) {
        checkListen();
        proxy.removeEntryListener(listener);
    }

    public void addEntryListener(EntryListener listener, Object key,
                                 boolean includeValue) {
        checkListen();
        proxy.addEntryListener(listener, key, includeValue);
    }

    public void clear() {
        checkRemove();
        proxy.clear();
    }

    public Set<Object> keySet() {
        checkGet();
        return proxy.keySet();
    }

    public void removeEntryListener(EntryListener listener, Object key) {
        proxy.removeEntryListener(listener, key);
    }

    public MapEntry getMapEntry(Object key) {
        checkGet();
        return proxy.getMapEntry(key);
    }

    public boolean evict(Object key) {
        checkRemove();
        return proxy.evict(key);
    }

    public Collection<Object> values() {
        checkGet();
        return proxy.values();
    }

    public Set<Object> keySet(Predicate predicate) {
        checkGet();
        return proxy.keySet(predicate);
    }

    public Set<java.util.Map.Entry> entrySet(Predicate predicate) {
        checkGet();
        return proxy.entrySet(predicate);
    }

    public Set<java.util.Map.Entry> entrySet() {
        checkGet();
        return proxy.entrySet();
    }

    public Collection<Object> values(Predicate predicate) {
        checkGet();
        return proxy.values(predicate);
    }

    public Set<Object> localKeySet() {
        checkGet();
        return proxy.localKeySet();
    }

    public Set<Object> localKeySet(Predicate predicate) {
        checkGet();
        return proxy.localKeySet(predicate);
    }

    public void addIndex(String attribute, boolean ordered) {
        proxy.addIndex(attribute, ordered);
    }

    public void addIndex(Expression expression, boolean ordered) {
        proxy.addIndex(expression, ordered);
    }

    public LocalMapStats getLocalMapStats() {
        SecurityUtil.checkPermission(node.securityContext, statsPermission);
        return proxy.getLocalMapStats();
    }

    // ------ MProxy

    public boolean removeKey(Object key) {
        checkRemove();
        return proxy.removeKey(key);
    }

    public String getLongName() {
        return proxy.getLongName();
    }

    public void addGenericListener(Object listener, Object key,
                                   boolean includeValue, InstanceType instanceType) {
        checkListen();
        proxy.addGenericListener(listener, key, includeValue, instanceType);
    }

    public void removeGenericListener(Object listener, Object key) {
        checkListen();
        proxy.removeGenericListener(listener, key);
    }

    public boolean containsEntry(Object key, Object value) {
        return proxy.containsEntry(key, value);
    }

    public boolean putMulti(Object key, Object value) {
        return proxy.putMulti(key, value);
    }

    public boolean removeMulti(Object key, Object value) {
        return proxy.removeMulti(key, value);
    }

    public boolean add(Object value) {
        return proxy.add(value);
    }

    public int valueCount(Object key) {
        return proxy.valueCount(key);
    }

    public Set allKeys() {
        return proxy.allKeys();
    }

    public MapOperationsCounter getMapOperationCounter() {
        return proxy.getMapOperationCounter();
    }

    public void putForSync(Object key, Object value) {
        proxy.putForSync(key, value);
    }

    public void removeForSync(Object key) {
        proxy.removeForSync(key);
    }

    public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
        proxy.setHazelcastInstance(hazelcastInstance);
    }
}
