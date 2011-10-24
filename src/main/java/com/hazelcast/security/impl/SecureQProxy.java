package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Iterator;
import java.util.concurrent.TimeUnit;

import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.IQueue;
import com.hazelcast.core.ItemListener;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.QProxy;
import com.hazelcast.impl.monitor.QueueOperationsCounter;
import com.hazelcast.monitor.LocalQueueStats;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.QueuePermission;

final class SecureQProxy<E> extends SecureProxySupport implements IQueue<E>, QProxy<E> {

	final QProxy<E> proxy;
	final QueuePermission offerPermission;
	final QueuePermission getPermission;
	final QueuePermission pollPermission;
	final QueuePermission removePermission;
	final QueuePermission listenPermission;
	final QueuePermission statsPermission;

	SecureQProxy(Node node, QProxy<E> queue) {
		super(node);
		this.proxy = queue;
		
		offerPermission = new QueuePermission(getName(), SecurityConstants.ACTION_OFFER);
		getPermission = new QueuePermission(getName(), SecurityConstants.ACTION_GET);
		pollPermission = new QueuePermission(getName(), SecurityConstants.ACTION_POLL);
		removePermission = new QueuePermission(getName(), SecurityConstants.ACTION_REMOVE);
		listenPermission = new QueuePermission(getName(), SecurityConstants.ACTION_LISTEN);
		statsPermission = new QueuePermission(getName(), SecurityConstants.ACTION_STATISTICS);
	}

	private void checkOffer() {
		SecurityUtil.checkPermission(node.securityContext, offerPermission);
	}
	private void checkGet() {
		SecurityUtil.checkPermission(node.securityContext, getPermission);
	}
	private void checkPoll() {
		SecurityUtil.checkPermission(node.securityContext, pollPermission);
	}
	private void checkRemove() {
		SecurityUtil.checkPermission(node.securityContext, removePermission);
	}
	private void checkListen() {
		SecurityUtil.checkPermission(node.securityContext, listenPermission);
	}
	
	// ------ IQueue
	
	public String getName() {
		return proxy.getName();
	}

	public LocalQueueStats getLocalQueueStats() {
		SecurityUtil.checkPermission(node.securityContext, statsPermission);
		return proxy.getLocalQueueStats();
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
		SecurityUtil.checkPermission(node.securityContext, new QueuePermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public Object getId() {
		return proxy.getId();
	}

	public boolean offer(E o) {
		checkOffer();
		return proxy.offer(o);
	}

	public E poll() {
		checkPoll();
		return proxy.poll();
	}

	public E remove() {
		checkRemove();
		return proxy.remove();
	}

	public boolean offer(E o, long timeout, TimeUnit unit)
			throws InterruptedException {
		checkOffer();
		return proxy.offer(o, timeout, unit);
	}

	public int size() {
		return proxy.size();
	}

	public E peek() {
		checkGet();
		return proxy.peek();
	}

	public E element() {
		checkGet();
		return proxy.element();
	}

	public boolean isEmpty() {
		checkGet();
		return proxy.isEmpty();
	}

	public boolean contains(Object o) {
		checkGet();
		return proxy.contains(o);
	}

	public E poll(long timeout, TimeUnit unit) throws InterruptedException {
		checkPoll();
		return proxy.poll(timeout, unit);
	}

	public E take() throws InterruptedException {
		checkPoll();
		return proxy.take();
	}

	public Iterator<E> iterator() {
		checkGet();
		return proxy.iterator();
	}

	public void put(E o) throws InterruptedException {
		checkOffer();
		proxy.put(o);
	}

	public Object[] toArray() {
		checkGet();
		return proxy.toArray();
	}

	public int remainingCapacity() {
		checkGet();
		return proxy.remainingCapacity();
	}

	public boolean add(E o) {
		checkOffer();
		return proxy.add(o);
	}

	public <T> T[] toArray(T[] a) {
		checkGet();
		return proxy.toArray(a);
	}

	public int drainTo(Collection<? super E> c) {
		checkPoll();
		return proxy.drainTo(c);
	}

	public int drainTo(Collection<? super E> c, int maxElements) {
		checkPoll();
		return proxy.drainTo(c, maxElements);
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
		checkOffer();
		return proxy.addAll(c);
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

	// ------ QProxy
	public QueueOperationsCounter getQueueOperationCounter() {
		return proxy.getQueueOperationCounter();
	}

	public String getLongName() {
		return proxy.getLongName();
	}
	
	public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
		proxy.setHazelcastInstance(hazelcastInstance);
	}
}
