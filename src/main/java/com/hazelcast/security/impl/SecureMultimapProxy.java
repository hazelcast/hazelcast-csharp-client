package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Set;
import java.util.concurrent.TimeUnit;

import com.hazelcast.core.EntryListener;
import com.hazelcast.impl.MProxy;
import com.hazelcast.impl.MultiMapProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.MultiMapPermission;

final class SecureMultimapProxy extends SecureProxySupport implements MultiMapProxy {

	final MultiMapProxy proxy; 
	
	SecureMultimapProxy(Node node, final MultiMapProxy proxy) {
		super(node);
		this.proxy = proxy;
	}
	
	private void checkPut() {
		checkPermission(new MultiMapPermission(getName(), SecurityConstants.ACTION_PUT));
	}
	
	private void checkGet() {
		checkPermission(new MultiMapPermission(getName(), SecurityConstants.ACTION_GET));
	}
	
	private void checkRemove() {
		checkPermission(new MultiMapPermission(getName(), SecurityConstants.ACTION_REMOVE));
	}
	
	private void checkListen() {
		checkPermission(new MultiMapPermission(getName(), SecurityConstants.ACTION_LISTEN));
	}
	
	private void checkLock() {
		checkPermission(new MultiMapPermission(getName(), SecurityConstants.ACTION_LOCK));
	}

	public MProxy getMProxy() {
		return new SecureMProxy(node, proxy.getMProxy());
	}

	public String getName() {
		return proxy.getName();
	}

	public boolean put(Object key, Object value) {
		checkPut();
		return proxy.put(key, value);
	}

	public Collection get(Object key) {
		checkGet();
		return proxy.get(key);
	}

	public boolean remove(Object key, Object value) {
		checkRemove();
		return proxy.remove(key, value);
	}

	public Collection remove(Object key) {
		checkRemove();
		return proxy.remove(key);
	}

	public InstanceType getInstanceType() {
		return proxy.getInstanceType();
	}

	public void destroy() {
		checkPermission(new MultiMapPermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public Set localKeySet() {
		checkGet();
		return proxy.localKeySet();
	}

	public Object getId() {
		return proxy.getId();
	}

	public Set keySet() {
		checkGet();
		return proxy.keySet();
	}

	public Collection values() {
		checkGet();
		return proxy.values();
	}

	public Set entrySet() {
		checkGet();
		return proxy.entrySet();
	}

	public boolean containsKey(Object key) {
		checkGet();
		return proxy.containsKey(key);
	}

	public boolean containsValue(Object value) {
		checkGet();
		return proxy.containsValue(value);
	}

	public boolean containsEntry(Object key, Object value) {
		checkGet();
		return proxy.containsEntry(key, value);
	}

	public int size() {
		checkGet();
		return proxy.size();
	}

	public void clear() {
		checkRemove();
		proxy.clear();
	}

	public int valueCount(Object key) {
		checkGet();
		return proxy.valueCount(key);
	}

	public void addLocalEntryListener(EntryListener listener) {
		checkListen();
		proxy.addLocalEntryListener(listener);
	}

	public void addEntryListener(EntryListener listener, boolean includeValue) {
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

	public void removeEntryListener(EntryListener listener, Object key) {
		checkListen();
		proxy.removeEntryListener(listener, key);
	}

	public void lock(Object key) {
		checkLock();
		proxy.lock(key);
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

	public boolean lockMap(long time, TimeUnit timeunit) {
		checkLock();
		return proxy.lockMap(time, timeunit);
	}

	public void unlockMap() {
		proxy.unlockMap();
	}
}
