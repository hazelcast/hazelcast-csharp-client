package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

import com.hazelcast.core.EntryListener;
import com.hazelcast.core.MapEntry;
import com.hazelcast.impl.MProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.monitor.MapOperationsCounter;
import com.hazelcast.monitor.LocalMapStats;
import com.hazelcast.query.Expression;
import com.hazelcast.query.Predicate;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.MapPermission;

final class SecureMProxy extends SecureProxySupport implements MProxy {
	
	private final MProxy map;

	SecureMProxy(Node node, MProxy map) {
		super(node);
		this.map = map;
	}
	
	private void checkPut() {
		checkPermission(new MapPermission(getName(), SecurityConstants.ACTION_PUT));
	}
	
	private void checkGet() {
		checkPermission(new MapPermission(getName(), SecurityConstants.ACTION_GET));
	}
	
	private void checkRemove() {
		checkPermission(new MapPermission(getName(), SecurityConstants.ACTION_REMOVE));
	}
	
	private void checkListen() {
		checkPermission(new MapPermission(getName(), SecurityConstants.ACTION_LISTEN));
	}
	
	private void checkLock() {
		checkPermission(new MapPermission(getName(), SecurityConstants.ACTION_LOCK));
	}

	// ------ IMap
	
	public Object putIfAbsent(Object key, Object value) {
		checkPut();
		return (Object) map.putIfAbsent(key, value);
	}

	public void flush() {
		map.flush();
	}

	public String getName() {
		return map.getName();
	}

	public InstanceType getInstanceType() {
		return map.getInstanceType();
	}

	public Map getAll(Set keys) {
		checkGet();
		return map.getAll(keys);
	}

	public boolean remove(Object key, Object value) {
		checkRemove();
		return map.remove(key, value);
	}

	public void destroy() {
		checkPermission(new MapPermission(getName(), SecurityConstants.ACTION_DESTROY));
		map.destroy();
	}

	public Future<Object> getAsync(Object key) {
		checkGet();
		return map.getAsync(key);
	}

	public Object getId() {
		return map.getId();
	}

	public boolean replace(Object key, Object oldValue, Object newValue) {
		checkPut();
		return map.replace(key, oldValue, newValue);
	}

	public Future<Object> putAsync(Object key, Object value) {
		checkPut();
		return map.putAsync(key, value);
	}

	public Object replace(Object key, Object value) {
		checkPut();
		return (Object) map.replace(key, value);
	}

	public Future<Object> removeAsync(Object key) {
		checkRemove();
		return map.removeAsync(key);
	}

	public Object tryRemove(Object key, long timeout, TimeUnit timeunit)
			throws TimeoutException {
		checkRemove();
		return map.tryRemove(key, timeout, timeunit);
	}

	public boolean tryPut(Object key, Object value, long timeout, TimeUnit timeunit) {
		checkPut();
		return map.tryPut(key, value, timeout, timeunit);
	}

	public int size() {
		checkGet();
		return map.size();
	}

	public boolean isEmpty() {
		checkGet();
		return map.isEmpty();
	}

	public boolean containsKey(Object key) {
		checkGet();
		return map.containsKey(key);
	}

	public Object put(Object key, Object value, long ttl, TimeUnit timeunit) {
		checkPut();
		return (Object) map.put(key, value, ttl, timeunit);
	}

	public void putTransient(Object key, Object value, long ttl, TimeUnit timeunit) {
		checkPut();
		map.putTransient(key, value, ttl, timeunit);
	}

	public boolean containsValue(Object value) {
		checkGet();
		return map.containsValue(value);
	}

	public Object putIfAbsent(Object key, Object value, long ttl, TimeUnit timeunit) {
		checkPut();
		return (Object) map.putIfAbsent(key, value, ttl, timeunit);
	}

	public Object tryLockAndGet(Object key, long time, TimeUnit timeunit)
			throws TimeoutException {
		checkGet();
		checkLock();
		return (Object) map.tryLockAndGet(key, time, timeunit);
	}

	public Object get(Object key) {
		checkGet();
		return (Object) map.get(key);
	}

	public void putAndUnlock(Object key, Object value) {
		checkPut();
		map.putAndUnlock(key, value);
	}

	public void lock(Object key) {
		checkLock();
		map.lock(key);
	}

	public Object put(Object key, Object value) {
		checkPut();
		return (Object) map.put(key, value);
	}

	public boolean tryLock(Object key) {
		checkLock();
		return map.tryLock(key);
	}

	public boolean tryLock(Object key, long time, TimeUnit timeunit) {
		checkLock();
		return map.tryLock(key, time, timeunit);
	}

	public void unlock(Object key) {
		map.unlock(key);
	}

	public boolean lockMap(long time, TimeUnit timeunit) {
		checkLock();
		return map.lockMap(time, timeunit);
	}

	public Object remove(Object key) {
		checkRemove();
		return (Object) map.remove(key);
	}

	public void unlockMap() {
		map.unlockMap();
	}

	public void addLocalEntryListener(EntryListener listener) {
		checkListen();
		map.addLocalEntryListener(listener);
	}

	public void putAll(Map t) {
		checkPut();
		map.putAll(t);
	}

	public void addEntryListener(EntryListener listener,
			boolean includeValue) {
		checkListen();
		map.addEntryListener(listener, includeValue);
	}

	public void removeEntryListener(EntryListener listener) {
		checkListen();
		map.removeEntryListener(listener);
	}

	public void addEntryListener(EntryListener listener, Object key,
			boolean includeValue) {
		checkListen();
		map.addEntryListener(listener, key, includeValue);
	}

	public void clear() {
		checkRemove();
		map.clear();
	}

	public Set<Object> keySet() {
		checkGet();
		return map.keySet();
	}

	public void removeEntryListener(EntryListener listener, Object key) {
		map.removeEntryListener(listener, key);
	}

	public MapEntry getMapEntry(Object key) {
		checkGet();
		return map.getMapEntry(key);
	}

	public boolean evict(Object key) {
		checkRemove();
		return map.evict(key);
	}

	public Collection<Object> values() {
		checkGet();
		return map.values();
	}

	public Set<Object> keySet(Predicate predicate) {
		checkGet();
		return map.keySet(predicate);
	}

	public Set<java.util.Map.Entry> entrySet(Predicate predicate) {
		checkGet();
		return map.entrySet(predicate);
	}

	public Set<java.util.Map.Entry> entrySet() {
		checkGet();
		return map.entrySet();
	}

	public Collection<Object> values(Predicate predicate) {
		checkGet();
		return map.values(predicate);
	}

	public Set<Object> localKeySet() {
		checkGet();
		return map.localKeySet();
	}

	public Set<Object> localKeySet(Predicate predicate) {
		checkGet();
		return map.localKeySet(predicate);
	}

	public void addIndex(String attribute, boolean ordered) {
		map.addIndex(attribute, ordered);
	}

	public void addIndex(Expression expression, boolean ordered) {
		map.addIndex(expression, ordered);
	}

	public LocalMapStats getLocalMapStats() {
		checkPermission(new MapPermission(getName(), SecurityConstants.ACTION_STATISTICS));
		return map.getLocalMapStats();
	}
	
	// ------ MProxy
	
	public boolean removeKey(Object key) {
		checkRemove();
		return map.removeKey(key);
	}

	public String getLongName() {
		return map.getLongName();
	}

	public void addGenericListener(Object listener, Object key,
			boolean includeValue, InstanceType instanceType) {
		checkListen();
		map.addGenericListener(listener, key, includeValue, instanceType);
	}

	public void removeGenericListener(Object listener, Object key) {
		checkListen();
		map.removeGenericListener(listener, key);
	}

	public boolean containsEntry(Object key, Object value) {
		return map.containsEntry(key, value);
	}

	public boolean putMulti(Object key, Object value) {
		return map.putMulti(key, value);
	}

	public boolean removeMulti(Object key, Object value) {
		return map.removeMulti(key, value);
	}

	public boolean add(Object value) {
		return map.add(value);
	}

	public int valueCount(Object key) {
		return map.valueCount(key);
	}

	public Set allKeys() {
		return map.allKeys();
	}

	public MapOperationsCounter getMapOperationCounter() {
		return map.getMapOperationCounter();
	}

	public void putForSync(Object key, Object value) {
		map.putForSync(key, value);
	}

	public void removeForSync(Object key) {
		map.removeForSync(key);
	}
}
