package com.hazelcast.security.impl;

import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.impl.AtomicNumberProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.monitor.AtomicNumberOperationsCounter;
import com.hazelcast.monitor.LocalAtomicNumberStats;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.AtomicNumberPermission;

final class SecureAtomicNumberProxy extends SecureProxySupport implements AtomicNumberProxy {
	
	final AtomicNumberProxy proxy;
	final AtomicNumberPermission addPermission;
	final AtomicNumberPermission setPermission;
	final AtomicNumberPermission incPermission;
	final AtomicNumberPermission decPermission;
	final AtomicNumberPermission statsPermission;
	
	SecureAtomicNumberProxy(Node node, final AtomicNumberProxy proxy) {
		super(node);
		this.proxy = proxy;
		addPermission = new AtomicNumberPermission(getName(), SecurityConstants.ACTION_ADD);
		setPermission = new AtomicNumberPermission(getName(), SecurityConstants.ACTION_SET);
		incPermission = new AtomicNumberPermission(getName(), SecurityConstants.ACTION_INCREMENT);
		decPermission = new AtomicNumberPermission(getName(), SecurityConstants.ACTION_DECREMENT);
		statsPermission = new AtomicNumberPermission(getName(), SecurityConstants.ACTION_STATISTICS);
	}
	
	private void checkAdd() {
		SecurityUtil.checkPermission(node.securityContext, addPermission);
	}
	private void checkSet() {
		SecurityUtil.checkPermission(node.securityContext, setPermission);
	}
	private void checkInc() {
		SecurityUtil.checkPermission(node.securityContext, incPermission);
	}
	private void checkDec() {
		SecurityUtil.checkPermission(node.securityContext, decPermission);
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
		SecurityUtil.checkPermission(node.securityContext, new AtomicNumberPermission(getName(), SecurityConstants.ACTION_DESTROY));
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
		SecurityUtil.checkPermission(node.securityContext, statsPermission);
		return proxy.getLocalAtomicNumberStats();
	}

	public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
		proxy.setHazelcastInstance(hazelcastInstance);
	}
}
