package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Iterator;

import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.ItemListener;
import com.hazelcast.impl.MProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.SetProxy;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.SetPermission;

final class SecureSetProxy<E> extends SecureProxySupport implements SetProxy<E> {
	
	private final SetProxy<E> proxy ;
	private final SetPermission addPermission ;
	private final SetPermission getPermission ;
	private final SetPermission removePermission ;
	private final SetPermission listenPermission ;
	
	SecureSetProxy(Node node, final SetProxy<E> set ) {
		super(node);
		proxy = set;
		addPermission = new SetPermission(getName(), SecurityConstants.ACTION_ADD);
		getPermission = new SetPermission(getName(), SecurityConstants.ACTION_GET);
		removePermission = new SetPermission(getName(), SecurityConstants.ACTION_REMOVE);
		listenPermission = new SetPermission(getName(), SecurityConstants.ACTION_LISTEN);
	}
	
	private void checkAdd() {
		SecurityUtil.checkPermission(node.securityContext, addPermission);
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

	public String getName() {
		return proxy.getName();
	}

	public void addItemListener(ItemListener<E> listener, boolean includeValue) {
		checkListen();
		proxy.addItemListener(listener, includeValue);
	}

	public void removeItemListener(ItemListener<E> listener) {
		checkListen();
		proxy.removeItemListener(listener);
	}

	public InstanceType getInstanceType() {
		return proxy.getInstanceType();
	}

	public void destroy() {
		SecurityUtil.checkPermission(node.securityContext, new SetPermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public Object getId() {
		return proxy.getId();
	}

	public int size() {
		checkGet();
		return proxy.size();
	}

	public boolean isEmpty() {
		checkGet();
		return proxy.isEmpty();
	}

	public boolean contains(Object o) {
		checkGet();
		return proxy.contains(o);
	}

	public Iterator<E> iterator() {
		checkGet();
		return proxy.iterator();
	}

	public Object[] toArray() {
		checkGet();
		return proxy.toArray();
	}

	public <T> T[] toArray(T[] a) {
		checkGet();
		return proxy.toArray(a);
	}

	public boolean add(E o) {
		checkGet();
		return proxy.add(o);
	}

	public boolean remove(Object o) {
		checkRemove();
		return proxy.remove(o);
	}

	public boolean containsAll(Collection<?> c) {
		checkGet();
		return proxy.containsAll(c);
	}

	public boolean addAll(Collection<? extends E> c) {
		checkAdd();
		return proxy.addAll(c);
	}

	public boolean retainAll(Collection<?> c) {
		checkRemove();
		return proxy.retainAll(c);
	}

	public boolean removeAll(Collection<?> c) {
		checkRemove();
		return proxy.removeAll(c);
	}

	public void clear() {
		checkRemove();
		proxy.clear();
	}
	
	public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
		proxy.setHazelcastInstance(hazelcastInstance);
	}

	public MProxy getMProxy() {
		return proxy.getMProxy();
	}
}
