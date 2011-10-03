package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Iterator;
import java.util.concurrent.TimeUnit;

import com.hazelcast.core.IQueue;
import com.hazelcast.core.ItemListener;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.QProxy;
import com.hazelcast.impl.monitor.QueueOperationsCounter;
import com.hazelcast.monitor.LocalQueueStats;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.QueuePermission;

final class SecureQProxy<E> extends SecureProxySupport implements IQueue<E>, QProxy<E> {

	final QProxy<E> queue;

	SecureQProxy(Node node, QProxy<E> queue) {
		super(node);
		this.queue = queue;
	}

	private void checkOffer() {
		checkPermission(new QueuePermission(getName(), SecurityConstants.ACTION_OFFER));
	}
	
	private void checkGet() {
		checkPermission(new QueuePermission(getName(), SecurityConstants.ACTION_GET));
	}
	
	private void checkPoll() {
		checkPermission(new QueuePermission(getName(), SecurityConstants.ACTION_POLL));
	}
	
	private void checkRemove() {
		checkPermission(new QueuePermission(getName(), SecurityConstants.ACTION_REMOVE));
	}
	
	private void checkListen() {
		checkPermission(new QueuePermission(getName(), SecurityConstants.ACTION_LISTEN));
	}
	
	// ------ IQueue
	
	public String getName() {
		return queue.getName();
	}

	public LocalQueueStats getLocalQueueStats() {
		checkPermission(new QueuePermission(getName(), SecurityConstants.ACTION_STATISTICS));
		return queue.getLocalQueueStats();
	}

	public void addItemListener(ItemListener<E> listener, boolean includeValue) {
		checkListen();
		queue.addItemListener(listener, includeValue);
	}

	public void removeItemListener(ItemListener<E> listener) {
		checkListen();
		queue.removeItemListener(listener);
	}

	public InstanceType getInstanceType() {
		return queue.getInstanceType();
	}

	public void destroy() {
		checkPermission(new QueuePermission(getName(), SecurityConstants.ACTION_DESTROY));
		queue.destroy();
	}

	public Object getId() {
		return queue.getId();
	}

	public boolean offer(E o) {
		checkOffer();
		return queue.offer(o);
	}

	public E poll() {
		checkPoll();
		return queue.poll();
	}

	public E remove() {
		checkRemove();
		return queue.remove();
	}

	public boolean offer(E o, long timeout, TimeUnit unit)
			throws InterruptedException {
		checkOffer();
		return queue.offer(o, timeout, unit);
	}

	public int size() {
		return queue.size();
	}

	public E peek() {
		checkGet();
		return queue.peek();
	}

	public E element() {
		checkGet();
		return queue.element();
	}

	public boolean isEmpty() {
		checkGet();
		return queue.isEmpty();
	}

	public boolean contains(Object o) {
		checkGet();
		return queue.contains(o);
	}

	public E poll(long timeout, TimeUnit unit) throws InterruptedException {
		checkPoll();
		return queue.poll(timeout, unit);
	}

	public E take() throws InterruptedException {
		checkPoll();
		return queue.take();
	}

	public Iterator<E> iterator() {
		checkGet();
		return queue.iterator();
	}

	public void put(E o) throws InterruptedException {
		checkOffer();
		queue.put(o);
	}

	public Object[] toArray() {
		checkGet();
		return queue.toArray();
	}

	public int remainingCapacity() {
		checkGet();
		return queue.remainingCapacity();
	}

	public boolean add(E o) {
		checkOffer();
		return queue.add(o);
	}

	public <T> T[] toArray(T[] a) {
		checkGet();
		return queue.toArray(a);
	}

	public int drainTo(Collection<? super E> c) {
		checkPoll();
		return queue.drainTo(c);
	}

	public int drainTo(Collection<? super E> c, int maxElements) {
		checkPoll();
		return queue.drainTo(c, maxElements);
	}

	public boolean remove(Object o) {
		checkRemove();
		return queue.remove(o);
	}

	public boolean containsAll(Collection<?> c) {
		checkGet();
		return queue.containsAll(c);
	}

	public boolean addAll(Collection<? extends E> c) {
		checkOffer();
		return queue.addAll(c);
	}

	public boolean removeAll(Collection<?> c) {
		checkRemove();
		return queue.removeAll(c);
	}

	public boolean retainAll(Collection<?> c) {
		checkRemove();
		return queue.retainAll(c);
	}

	public void clear() {
		checkRemove();
		queue.clear();
	}


	// ------ QProxy
	public QueueOperationsCounter getQueueOperationCounter() {
		return queue.getQueueOperationCounter();
	}

	public String getLongName() {
		return queue.getLongName();
	}
}
