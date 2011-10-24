package com.hazelcast.security.impl;

import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;

import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.InstanceDestroyedException;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.SemaphoreProxy;
import com.hazelcast.impl.monitor.SemaphoreOperationsCounter;
import com.hazelcast.monitor.LocalSemaphoreStats;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.SemaphorePermission;

final class SecureSemaphoreProxy extends SecureProxySupport implements SemaphoreProxy {
	
	final SemaphoreProxy proxy;
	final SemaphorePermission acquirePermission;
	final SemaphorePermission releasePermission;
	final SemaphorePermission drainPermission;
	final SemaphorePermission statsPermission;
	
	SecureSemaphoreProxy(Node node, final SemaphoreProxy proxy) {
		super(node);
		this.proxy = proxy;
		acquirePermission = new SemaphorePermission(getName(), SecurityConstants.ACTION_ACQUIRE);
		releasePermission = new SemaphorePermission(getName(), SecurityConstants.ACTION_RELEASE);
		drainPermission = new SemaphorePermission(getName(), SecurityConstants.ACTION_DRAIN);
		statsPermission = new SemaphorePermission(getName(), SecurityConstants.ACTION_STATISTICS);
	}
	
	private void checkAcquire() {
		SecurityUtil.checkPermission(node.securityContext, acquirePermission);
	}
	
	private void checkRelease() {
		SecurityUtil.checkPermission(node.securityContext, releasePermission);
	}
	
	private void checkDrain() {
		SecurityUtil.checkPermission(node.securityContext, drainPermission);
	}

	public SemaphoreOperationsCounter getOperationsCounter() {
		return proxy.getOperationsCounter();
	}

	public String getLongName() {
		return proxy.getLongName();
	}

	public InstanceType getInstanceType() {
		return proxy.getInstanceType();
	}

	public void destroy() {
		SecurityUtil.checkPermission(node.securityContext, new SemaphorePermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public Object getId() {
		return proxy.getId();
	}

	public String getName() {
		return proxy.getName();
	}

	public void acquire() throws InstanceDestroyedException,
			InterruptedException {
		checkAcquire();
		proxy.acquire();
	}

	public void acquire(int permits) throws InstanceDestroyedException,
			InterruptedException {
		checkAcquire();
		proxy.acquire(permits);
	}

	public Future acquireAsync() {
		checkAcquire();
		return proxy.acquireAsync();
	}

	public Future acquireAsync(int permits) {
		checkAcquire();
		return proxy.acquireAsync(permits);
	}

	public void acquireAttach() throws InstanceDestroyedException,
			InterruptedException {
		checkAcquire();
		proxy.acquireAttach();
	}

	public void acquireAttach(int permits) throws InstanceDestroyedException,
			InterruptedException {
		checkAcquire();
		proxy.acquireAttach(permits);
	}

	public Future acquireAttachAsync() {
		checkAcquire();
		return proxy.acquireAttachAsync();
	}

	public Future acquireAttachAsync(int permits) {
		checkAcquire();
		return proxy.acquireAttachAsync(permits);
	}

	public void attach() {
		proxy.attach();
	}

	public void attach(int permits) {
		proxy.attach(permits);
	}

	public int attachedPermits() {
		return proxy.attachedPermits();
	}

	public int availablePermits() {
		return proxy.availablePermits();
	}

	public void detach() {
		proxy.detach();
	}

	public void detach(int permits) {
		proxy.detach(permits);
	}

	public int drainPermits() {
		checkDrain();
		return proxy.drainPermits();
	}

	public void reducePermits(int reduction) {
		proxy.reducePermits(reduction);
	}

	public void release() {
		checkRelease();
		proxy.release();
	}

	public void release(int permits) {
		checkRelease();
		proxy.release(permits);
	}

	public void releaseDetach() {
		checkRelease();
		proxy.releaseDetach();
	}

	public void releaseDetach(int permits) {
		checkRelease();
		proxy.releaseDetach(permits);
	}

	public boolean tryAcquire() {
		checkAcquire();
		return proxy.tryAcquire();
	}

	public boolean tryAcquire(int permits) {
		checkAcquire();
		return proxy.tryAcquire(permits);
	}

	public boolean tryAcquire(long timeout, TimeUnit unit)
			throws InstanceDestroyedException, InterruptedException {
		checkAcquire();
		return proxy.tryAcquire(timeout, unit);
	}

	public boolean tryAcquire(int permits, long timeout, TimeUnit unit)
			throws InstanceDestroyedException, InterruptedException {
		checkAcquire();
		return proxy.tryAcquire(permits, timeout, unit);
	}

	public boolean tryAcquireAttach() {
		checkAcquire();
		return proxy.tryAcquireAttach();
	}

	public boolean tryAcquireAttach(int permits) {
		checkAcquire();
		return proxy.tryAcquireAttach(permits);
	}

	public boolean tryAcquireAttach(long timeout, TimeUnit unit)
			throws InstanceDestroyedException, InterruptedException {
		checkAcquire();
		return proxy.tryAcquireAttach(timeout, unit);
	}

	public boolean tryAcquireAttach(int permits, long timeout, TimeUnit unit)
			throws InstanceDestroyedException, InterruptedException {
		checkAcquire();
		return proxy.tryAcquireAttach(permits, timeout, unit);
	}

	public LocalSemaphoreStats getLocalSemaphoreStats() {
		SecurityUtil.checkPermission(node.securityContext, statsPermission);
		return proxy.getLocalSemaphoreStats();
	}
	
	public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
		proxy.setHazelcastInstance(hazelcastInstance);
	}
}
