package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

import com.hazelcast.impl.ExecutorServiceProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.ExecutorServicePermission;

final class SecureExecutorServiceProxy extends SecureProxySupport implements ExecutorService {
	
	final ExecutorServiceProxy proxy;
	
	SecureExecutorServiceProxy(Node node, final ExecutorServiceProxy proxy) {
		super(node);
		this.proxy = proxy;
	}

	private void checkExecute() {
		checkPermission(new ExecutorServicePermission(proxy.getName(), SecurityConstants.ACTION_EXECUTE));
	}
	
	public boolean awaitTermination(long timeout, TimeUnit unit)
			throws InterruptedException {
		return proxy.awaitTermination(timeout, unit);
	}

	public List<Future> invokeAll(Collection tasks) throws InterruptedException {
		checkExecute();
		return proxy.invokeAll(tasks);
	}

	public List invokeAll(Collection tasks, long timeout, TimeUnit unit)
			throws InterruptedException {
		checkExecute();
		return proxy.invokeAll(tasks, timeout, unit);
	}

	public Object invokeAny(Collection tasks) throws InterruptedException,
			ExecutionException {
		checkExecute();
		return proxy.invokeAny(tasks);
	}

	public Object invokeAny(Collection tasks, long timeout, TimeUnit unit)
			throws InterruptedException, ExecutionException, TimeoutException {
		checkExecute();
		return proxy.invokeAny(tasks, timeout, unit);
	}

	public boolean isShutdown() {
		return proxy.isShutdown();
	}

	public boolean isTerminated() {
		return proxy.isTerminated();
	}

	public void shutdown() {
		checkPermission(new ExecutorServicePermission(proxy.getName(), SecurityConstants.ACTION_DESTROY));
		proxy.shutdown();
	}

	public List<Runnable> shutdownNow() {
		checkPermission(new ExecutorServicePermission(proxy.getName(), SecurityConstants.ACTION_DESTROY));
		return proxy.shutdownNow();
	}

	public <T> Future<T> submit(Callable<T> task) {
		checkExecute();
		return proxy.submit(task);
	}

	public Future<?> submit(Runnable task) {
		checkExecute();
		return proxy.submit(task);
	}

	public <T> Future<T> submit(Runnable task, T result) {
		checkExecute();
		return proxy.submit(task, result);
	}

	public void execute(Runnable command) {
		checkExecute();
		proxy.execute(command);
	}
}
