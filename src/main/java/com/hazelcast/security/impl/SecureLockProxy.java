package com.hazelcast.security.impl;

import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.Condition;

import com.hazelcast.impl.LockProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.monitor.LockOperationsCounter;
import com.hazelcast.monitor.LocalLockStats;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.LockPermission;

final class SecureLockProxy extends SecureProxySupport implements LockProxy {
	
	final LockProxy proxy;
	
	SecureLockProxy(Node node, final LockProxy proxy) {
		super(node);
		this.proxy = proxy;
	}
	
	private void checkLock() {
		checkPermission(new LockPermission(getName(), SecurityConstants.ACTION_LOCK));
	}
	
	private final String getName() {
		return getLockObject().toString();
	}
	
	public Object getLockObject() {
		return proxy.getLockObject();
	}

	public LockOperationsCounter getLockOperationCounter() {
		return proxy.getLockOperationCounter();
	}

	public LocalLockStats getLocalLockStats() {
		checkPermission(new LockPermission(getName(), SecurityConstants.ACTION_STATISTICS));
		return proxy.getLocalLockStats();
	}

	public InstanceType getInstanceType() {
		return proxy.getInstanceType();
	}

	public void destroy() {
		checkPermission(new LockPermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public Object getId() {
		return proxy.getId();
	}

	public void lock() {
		checkLock();
		proxy.lock();
	}

	public void lockInterruptibly() throws InterruptedException {
		checkLock();
		proxy.lockInterruptibly();
	}

	public boolean tryLock() {
		checkLock();
		return proxy.tryLock();
	}

	public boolean tryLock(long time, TimeUnit unit)
			throws InterruptedException {
		checkLock();
		return proxy.tryLock(time, unit);
	}

	public void unlock() {
		proxy.unlock();
	}

	public Condition newCondition() {
		return proxy.newCondition();
	}

}
