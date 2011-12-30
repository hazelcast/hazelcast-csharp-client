package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;

import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.ItemListener;
import com.hazelcast.impl.ListProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.ListPermission;

final class SecureListProxy<E> extends SecureProxySupport implements ListProxy<E> {
	
	private final ListProxy<E> proxy;
	private final ListPermission addPermission ;
	private final ListPermission setPermission ;
	private final ListPermission getPermission ;
	private final ListPermission removePermission ;
	private final ListPermission listenPermission ;
	
	SecureListProxy(Node node, final ListProxy<E> list) {
		super(node);
		this.proxy = list;
		addPermission = new ListPermission(getName(), SecurityConstants.ACTION_ADD);
		getPermission = new ListPermission(getName(), SecurityConstants.ACTION_GET);
		setPermission = new ListPermission(getName(), SecurityConstants.ACTION_SET);
		removePermission = new ListPermission(getName(), SecurityConstants.ACTION_REMOVE);
		listenPermission = new ListPermission(getName(), SecurityConstants.ACTION_LISTEN);
	}
	
	private void checkAdd() {
		SecurityUtil.checkPermission(node.securityContext, addPermission);
	}
	private void checkSet() {
		SecurityUtil.checkPermission(node.securityContext, setPermission);
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
		SecurityUtil.checkPermission(node.securityContext, new ListPermission(getName(), SecurityConstants.ACTION_DESTROY));
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
		checkAdd();
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

	public boolean addAll(int index, Collection<? extends E> c) {
		checkAdd();
		return proxy.addAll(index, c);
	}

	public boolean removeAll(Collection<?> c) {
		checkRemove();
		return proxy.removeAll(c);
	}

	public boolean retainAll(Collection<?> c) {
		checkRemove();
		return proxy.retainAll(c);
	}

	public void clear() {
		checkRemove();
		proxy.clear();
	}

	public E get(int index) {
		checkGet();
		return proxy.get(index);
	}

	public E set(int index, E element) {
		checkSet();
		return proxy.set(index, element);
	}

	public void add(int index, E element) {
		checkAdd();
		proxy.add(index, element);
	}

	public E remove(int index) {
		checkRemove();
		return proxy.remove(index);
	}

	public int indexOf(Object o) {
		checkGet();
		return proxy.indexOf(o);
	}

	public int lastIndexOf(Object o) {
		checkGet();
		return proxy.lastIndexOf(o);
	}

	public ListIterator<E> listIterator() {
		checkGet();
		return proxy.listIterator();
	}

	public ListIterator<E> listIterator(int index) {
		checkGet();
		return proxy.listIterator(index);
	}

	public List<E> subList(int fromIndex, int toIndex) {
		checkGet();
		return proxy.subList(fromIndex, toIndex);
	}
	
	public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
		proxy.setHazelcastInstance(hazelcastInstance);
	}
}
