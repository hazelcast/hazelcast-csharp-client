package com.hazelcast.security.impl;

import java.util.Collection;
import java.util.Set;
import java.util.concurrent.ExecutorService;

import com.hazelcast.config.Config;
import com.hazelcast.core.AtomicNumber;
import com.hazelcast.core.Cluster;
import com.hazelcast.core.ICountDownLatch;
import com.hazelcast.core.IList;
import com.hazelcast.core.ILock;
import com.hazelcast.core.IMap;
import com.hazelcast.core.IQueue;
import com.hazelcast.core.ISemaphore;
import com.hazelcast.core.ISet;
import com.hazelcast.core.ITopic;
import com.hazelcast.core.IdGenerator;
import com.hazelcast.core.Instance;
import com.hazelcast.core.InstanceListener;
import com.hazelcast.core.LifecycleService;
import com.hazelcast.core.MultiMap;
import com.hazelcast.core.Prefix;
import com.hazelcast.core.Transaction;
import com.hazelcast.impl.FactoryImpl.HazelcastInstanceProxy;
import com.hazelcast.impl.FactoryImpl.ProxyKey;
import com.hazelcast.impl.HazelcastInstanceAwareInstance;
import com.hazelcast.impl.IHazelcastFactory;
import com.hazelcast.impl.MProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.QProxy;
import com.hazelcast.logging.LoggingService;
import com.hazelcast.partition.PartitionService;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.ClusterPermission;
import com.hazelcast.security.permission.MapPermission;
import com.hazelcast.security.permission.QueuePermission;

public class SecureHazelcastFactory implements IHazelcastFactory {

	private final Node node;
	private final IHazelcastFactory factory;

	public SecureHazelcastFactory(Node node) {
		super();
		this.node = node;
		this.factory = node.factory;
	}

	public Object getOrCreateProxyByName(String name) {
		if (!doesProxyExist(name)) {
			checkInstancePermission(name, SecurityConstants.ACTION_CREATE);
		}
		return getSecureProxy(factory.getOrCreateProxyByName(name));
	}

	public Object getOrCreateProxy(ProxyKey proxyKey) {
		if (!doesProxyExist(proxyKey.getName())) {
			checkInstancePermission(proxyKey.getName(), SecurityConstants.ACTION_CREATE);
		}
		return getSecureProxy(factory.getOrCreateProxy(proxyKey));
	}

	public void destroyProxy(ProxyKey proxyKey) {
		checkInstancePermission(proxyKey.getName(), SecurityConstants.ACTION_DESTROY);
		factory.destroyProxy(proxyKey);
	}

	public <K, V> IMap<K, V> getMap(String name) {
		return (IMap<K, V>) getOrCreateProxyByName(Prefix.MAP + name);
	}

	public <E> IQueue<E> getQueue(String name) {
		return (IQueue) getOrCreateProxyByName(Prefix.QUEUE + name);
	}

	public <E> ITopic<E> getTopic(String name) {
		return (ITopic<E>) getOrCreateProxyByName(Prefix.TOPIC + name);
	}

	public <E> ISet<E> getSet(String name) {
		return (ISet<E>) getOrCreateProxyByName(Prefix.SET + name);
	}

	public <E> IList<E> getList(String name) {
		return (IList<E>) getOrCreateProxyByName(Prefix.AS_LIST + name);
	}

	public <K, V> MultiMap<K, V> getMultiMap(String name) {
		return (MultiMap<K, V>) getOrCreateProxyByName(Prefix.MULTIMAP + name);
	}

	public ILock getLock(Object key) {
		return (ILock) getOrCreateProxy(new ProxyKey("lock", key));
	}

	public ExecutorService getExecutorService() {
		return factory.getExecutorService();
	}

	public ExecutorService getExecutorService(String name) {
		return factory.getExecutorService(name);
	}

	public IdGenerator getIdGenerator(String name) {
		return factory.getIdGenerator(name);
	}

	public AtomicNumber getAtomicNumber(String name) {
		return factory.getAtomicNumber(name);
	}

	public ICountDownLatch getCountDownLatch(String name) {
		return factory.getCountDownLatch(name);
	}

	public ISemaphore getSemaphore(String name) {
		return factory.getSemaphore(name);
	}

	public Transaction getTransaction() {
		return factory.getTransaction();
	}

	public PartitionService getPartitionService() {
		return factory.getPartitionService();
	}

	public LoggingService getLoggingService() {
		return factory.getLoggingService();
	}

	public LifecycleService getLifecycleService() {
		return factory.getLifecycleService();
	}

	public void restart() {
		throw new UnsupportedOperationException();
	}

	public void shutdown() {
		throw new UnsupportedOperationException();
	}

	public void addInstanceListener(InstanceListener instanceListener) {
		factory.addInstanceListener(instanceListener);
	}

	public void removeInstanceListener(InstanceListener instanceListener) {
		factory.removeInstanceListener(instanceListener);
	}

	public String getName() {
		return factory.getName();
	}

	public Config getConfig() {
		return factory.getConfig();
	}

	public Collection<Instance> getInstances() {
		return factory.getInstances();
	}

	public Cluster getCluster() {
		return factory.getCluster();
	}

	public HazelcastInstanceProxy getHazelcastInstanceProxy() {
		return factory.getHazelcastInstanceProxy();
	}

	public Set<String> getLongInstanceNames() {
		return factory.getLongInstanceNames();
	}

	public Collection<HazelcastInstanceAwareInstance> getProxies() {
		return factory.getProxies();
	}

	private Object getSecureProxy(Object proxy) {
		if (proxy instanceof MProxy) {
			return new SecureMProxy(node, (MProxy) proxy);
		} else if (proxy instanceof QProxy) {
			return new SecureQProxy(node, (QProxy) proxy);
		}
		return proxy;
	}

	private void checkInstancePermission(String name, String action) {
		ClusterPermission p = null;
		int ix = name.lastIndexOf(':');
		String actualName = name;
		if (ix > -1) {
			actualName = actualName.substring(ix + 1);
		}

		if (name.startsWith(Prefix.MAP)) {
			p = new MapPermission(actualName, action);
		} else if (name.startsWith(Prefix.QUEUE)) {
			p = new QueuePermission(actualName, action);
		} else if (name.startsWith(Prefix.TOPIC)) {
		} else if (name.startsWith(Prefix.AS_LIST)) {
		} else if (name.startsWith(Prefix.MULTIMAP)) {
		} else if (name.startsWith(Prefix.SET)) {
		} else if (name.startsWith(Prefix.ATOMIC_NUMBER)) {
		} else if (name.startsWith(Prefix.IDGEN)) {
		} else if (name.startsWith(Prefix.SEMAPHORE)) {
		} else if (name.startsWith(Prefix.COUNT_DOWN_LATCH)) {
		} else if (name.equals("lock")) {
		}

		if (p != null && node.securityContext != null) {
			node.securityContext.checkPermission(p);
		}
	}

	private boolean doesProxyExist(String name) {
		final Collection<HazelcastInstanceAwareInstance> c = factory.getProxies();
		for (HazelcastInstanceAwareInstance ins : c) {
			if (ins.getId().equals(name)) {
				return true;
			}
		}
		return false;
	}
}
