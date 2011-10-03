package com.hazelcast.security.impl;

import com.hazelcast.impl.AtomicNumberProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.monitor.AtomicNumberOperationsCounter;
import com.hazelcast.monitor.LocalAtomicNumberStats;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.AtomicNumberPermission;

final class SecureAtomicNumberProxy extends SecureProxySupport implements AtomicNumberProxy {
	
	final AtomicNumberProxy proxy;
	
	SecureAtomicNumberProxy(Node node, final AtomicNumberProxy proxy) {
		super(node);
		this.proxy = proxy;
	}
	
	private void checkAdd() {
		checkPermission(new AtomicNumberPermission(getName(), SecurityConstants.ACTION_ADD));
	}
	
	private void checkSet() {
		checkPermission(new AtomicNumberPermission(getName(), SecurityConstants.ACTION_SET));
	}
	
	private void checkInc() {
		checkPermission(new AtomicNumberPermission(getName(), SecurityConstants.ACTION_INCREMENT));
	}
	
	private void checkDec() {
		checkPermission(new AtomicNumberPermission(getName(), SecurityConstants.ACTION_DECREMENT));
	}

	public String getName() {
		return proxy.getName();
	}

	public AtomicNumberOperationsCounter getOperationsCounter() {
		return proxy.getOperationsCounter();
	}

	public String getLongName() {
		return proxy.getLongName();
	}

	public long addAndGet(long delta) {
		checkAdd();
		return proxy.addAndGet(delta);
	}

	public boolean compareAndSet(long expect, long update) {
		checkSet();
		return proxy.compareAndSet(expect, update);
	}

	public long decrementAndGet() {
		checkDec();
		return proxy.decrementAndGet();
	}

	public long get() {
		return proxy.get();
	}

	public long getAndAdd(long delta) {
		checkAdd();
		return proxy.getAndAdd(delta);
	}

	public InstanceType getInstanceType() {
		return proxy.getInstanceType();
	}

	public long getAndSet(long newValue) {
		checkSet();
		return proxy.getAndSet(newValue);
	}

	public void destroy() {
		checkPermission(new AtomicNumberPermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public long incrementAndGet() {
		checkInc();
		return proxy.incrementAndGet();
	}

	public Object getId() {
		return proxy.getId();
	}

	public void set(long newValue) {
		checkSet();
		proxy.set(newValue);
	}

	public boolean weakCompareAndSet(long expect, long update) {
		checkSet();
		return proxy.weakCompareAndSet(expect, update);
	}

	public void lazySet(long newValue) {
		checkSet();
		proxy.lazySet(newValue);
	}

	public LocalAtomicNumberStats getLocalAtomicNumberStats() {
		checkPermission(new AtomicNumberPermission(getName(), SecurityConstants.ACTION_STATISTICS));
		return proxy.getLocalAtomicNumberStats();
	}
}
