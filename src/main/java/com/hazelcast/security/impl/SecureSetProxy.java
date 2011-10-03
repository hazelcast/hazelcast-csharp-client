package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Iterator;

import com.hazelcast.core.ISet;
import com.hazelcast.core.ItemListener;
import com.hazelcast.impl.Node;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.SetPermission;

final class SecureSetProxy<E> extends SecureProxySupport implements ISet<E> {
	
	final ISet<E> set ;
	
	SecureSetProxy(Node node, final ISet<E> set ) {
		super(node);
		this.set = set;
	}
	
	private void checkAdd() {
		checkPermission(new SetPermission(getName(), SecurityConstants.ACTION_ADD));
	}
	
	private void checkGet() {
		checkPermission(new SetPermission(getName(), SecurityConstants.ACTION_GET));
	}
	
	private void checkRemove() {
		checkPermission(new SetPermission(getName(), SecurityConstants.ACTION_REMOVE));
	}
	
	private void checkListen() {
		checkPermission(new SetPermission(getName(), SecurityConstants.ACTION_LISTEN));
	}

	public String getName() {
		return set.getName();
	}

	public void addItemListener(ItemListener<E> listener, boolean includeValue) {
		checkListen();
		set.addItemListener(listener, includeValue);
	}

	public void removeItemListener(ItemListener<E> listener) {
		checkListen();
		set.removeItemListener(listener);
	}

	public InstanceType getInstanceType() {
		return set.getInstanceType();
	}

	public void destroy() {
		checkPermission(new SetPermission(getName(), SecurityConstants.ACTION_DESTROY));
		set.destroy();
	}

	public Object getId() {
		return set.getId();
	}

	public int size() {
		checkGet();
		return set.size();
	}

	public boolean isEmpty() {
		checkGet();
		return set.isEmpty();
	}

	public boolean contains(Object o) {
		checkGet();
		return set.contains(o);
	}

	public Iterator<E> iterator() {
		checkGet();
		return set.iterator();
	}

	public Object[] toArray() {
		checkGet();
		return set.toArray();
	}

	public <T> T[] toArray(T[] a) {
		checkGet();
		return set.toArray(a);
	}

	public boolean add(E o) {
		checkGet();
		return set.add(o);
	}

	public boolean remove(Object o) {
		checkRemove();
		return set.remove(o);
	}

	public boolean containsAll(Collection<?> c) {
		checkGet();
		return set.containsAll(c);
	}

	public boolean addAll(Collection<? extends E> c) {
		checkAdd();
		return set.addAll(c);
	}

	public boolean retainAll(Collection<?> c) {
		checkRemove();
		return set.retainAll(c);
	}

	public boolean removeAll(Collection<?> c) {
		checkRemove();
		return set.removeAll(c);
	}

	public void clear() {
		checkRemove();
		set.clear();
	}
}
