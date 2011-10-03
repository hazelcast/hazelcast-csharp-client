package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;

import com.hazelcast.core.IList;
import com.hazelcast.core.ItemListener;
import com.hazelcast.impl.Node;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.ListPermission;

final class SecureListProxy<E> extends SecureProxySupport implements IList<E> {
	
	private final IList<E> list;
	
	SecureListProxy(Node node, final IList<E> list) {
		super(node);
		this.list = list;
	}
	
	private void checkAdd() {
		checkPermission(new ListPermission(getName(), SecurityConstants.ACTION_ADD));
	}
	
	private void checkSet() {
		checkPermission(new ListPermission(getName(), SecurityConstants.ACTION_SET));
	}
	
	private void checkGet() {
		checkPermission(new ListPermission(getName(), SecurityConstants.ACTION_GET));
	}
	
	private void checkRemove() {
		checkPermission(new ListPermission(getName(), SecurityConstants.ACTION_REMOVE));
	}
	
	private void checkListen() {
		checkPermission(new ListPermission(getName(), SecurityConstants.ACTION_LISTEN));
	}

	public String getName() {
		return list.getName();
	}

	public void addItemListener(ItemListener<E> listener, boolean includeValue) {
		checkListen();
		list.addItemListener(listener, includeValue);
	}

	public void removeItemListener(ItemListener<E> listener) {
		checkListen();
		list.removeItemListener(listener);
	}

	public InstanceType getInstanceType() {
		return list.getInstanceType();
	}

	public void destroy() {
		checkPermission(new ListPermission(getName(), SecurityConstants.ACTION_DESTROY));
		list.destroy();
	}

	public Object getId() {
		return list.getId();
	}

	public int size() {
		checkGet();
		return list.size();
	}

	public boolean isEmpty() {
		checkGet();
		return list.isEmpty();
	}

	public boolean contains(Object o) {
		checkGet();
		return list.contains(o);
	}

	public Iterator<E> iterator() {
		checkGet();
		return list.iterator();
	}

	public Object[] toArray() {
		checkGet();
		return list.toArray();
	}

	public <T> T[] toArray(T[] a) {
		checkGet();
		return list.toArray(a);
	}

	public boolean add(E o) {
		checkAdd();
		return list.add(o);
	}

	public boolean remove(Object o) {
		checkRemove();
		return list.remove(o);
	}

	public boolean containsAll(Collection<?> c) {
		checkGet();
		return list.containsAll(c);
	}

	public boolean addAll(Collection<? extends E> c) {
		checkAdd();
		return list.addAll(c);
	}

	public boolean addAll(int index, Collection<? extends E> c) {
		checkAdd();
		return list.addAll(index, c);
	}

	public boolean removeAll(Collection<?> c) {
		checkRemove();
		return list.removeAll(c);
	}

	public boolean retainAll(Collection<?> c) {
		checkRemove();
		return list.retainAll(c);
	}

	public void clear() {
		checkRemove();
		list.clear();
	}

	public E get(int index) {
		checkGet();
		return list.get(index);
	}

	public E set(int index, E element) {
		checkSet();
		return list.set(index, element);
	}

	public void add(int index, E element) {
		checkAdd();
		list.add(index, element);
	}

	public E remove(int index) {
		checkRemove();
		return list.remove(index);
	}

	public int indexOf(Object o) {
		checkGet();
		return list.indexOf(o);
	}

	public int lastIndexOf(Object o) {
		checkGet();
		return list.lastIndexOf(o);
	}

	public ListIterator<E> listIterator() {
		checkGet();
		return list.listIterator();
	}

	public ListIterator<E> listIterator(int index) {
		checkGet();
		return list.listIterator(index);
	}

	public List<E> subList(int fromIndex, int toIndex) {
		checkGet();
		return list.subList(fromIndex, toIndex);
	}
}
