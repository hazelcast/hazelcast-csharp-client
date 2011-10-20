package com.hazelcast.security.impl;

import java.security.Permission;
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
import com.hazelcast.impl.AtomicNumberProxy;
import com.hazelcast.impl.CountDownLatchProxy;
import com.hazelcast.impl.ExecutorServiceProxy;
import com.hazelcast.impl.FactoryImpl.HazelcastInstanceProxy;
import com.hazelcast.impl.FactoryImpl.ProxyKey;
import com.hazelcast.impl.HazelcastInstanceAwareInstance;
import com.hazelcast.impl.IHazelcastFactory;
import com.hazelcast.impl.LockProxy;
import com.hazelcast.impl.MProxy;
import com.hazelcast.impl.MultiMapProxy;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.QProxy;
import com.hazelcast.impl.SemaphoreProxy;
import com.hazelcast.impl.TopicProxy;
import com.hazelcast.logging.LoggingService;
import com.hazelcast.partition.PartitionService;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.AtomicNumberPermission;
import com.hazelcast.security.permission.ClusterPermission;
import com.hazelcast.security.permission.CountDownLatchPermission;
import com.hazelcast.security.permission.ExecutorServicePermission;
import com.hazelcast.security.permission.ListPermission;
import com.hazelcast.security.permission.ListenerPermission;
import com.hazelcast.security.permission.LockPermission;
import com.hazelcast.security.permission.MapPermission;
import com.hazelcast.security.permission.MultiMapPermission;
import com.hazelcast.security.permission.QueuePermission;
import com.hazelcast.security.permission.SemaphorePermission;
import com.hazelcast.security.permission.SetPermission;
import com.hazelcast.security.permission.TopicPermission;
import com.hazelcast.security.permission.TransactionPermission;

public class SecureHazelcastFactory implements IHazelcastFactory {

	private final Node node;
	private final IHazelcastFactory factory;

	public SecureHazelcastFactory(Node node) {
		super();
		this.node = node;
		this.factory = node.factory;
	}

	public Object getOrCreateProxyByName(String name) {
		if (!containsInstanceProxy(name)) {
			checkInstancePermission(name, SecurityConstants.ACTION_CREATE);
		}
		return getSecureProxy(factory.getOrCreateProxyByName(name));
	}

	public Object getOrCreateProxy(ProxyKey proxyKey) {
		if (!containsInstanceProxy(proxyKey.getName())) {
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
		final ProxyKey proxyKey = new ProxyKey("lock", key);
		if (!containsInstanceProxy(proxyKey)) {
			checkPermission(new LockPermission(key.toString(), SecurityConstants.ACTION_CREATE));
		}
		return new SecureLockProxy(node, (LockProxy) factory.getLock(key));
	}

	public ExecutorService getExecutorService() {
		return getExecutorService("default");
	}

	public ExecutorService getExecutorService(String name) {
		if (!containsExecutorServiceProxy(Prefix.EXECUTOR_SERVICE + name)) {
			checkPermission(new ExecutorServicePermission(name, SecurityConstants.ACTION_CREATE));
		}
		return new SecureExecutorServiceProxy(node, (ExecutorServiceProxy) factory.getExecutorService(name));
	}

	public IdGenerator getIdGenerator(String name) {
		return factory.getIdGenerator(name);
	}

	public AtomicNumber getAtomicNumber(String name) {
		return (AtomicNumber) getOrCreateProxyByName(Prefix.ATOMIC_NUMBER + name);
	}

	public ICountDownLatch getCountDownLatch(String name) {
		return (ICountDownLatch) getOrCreateProxyByName(Prefix.COUNT_DOWN_LATCH + name);
	}

	public ISemaphore getSemaphore(String name) {
        return (ISemaphore) getOrCreateProxyByName(Prefix.SEMAPHORE + name);
	}

	public Transaction getTransaction() {
		checkPermission(new TransactionPermission());
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
		checkPermission(new ListenerPermission(SecurityConstants.LISTENER_INSTANCE));
		factory.addInstanceListener(instanceListener);
	}

	public void removeInstanceListener(InstanceListener instanceListener) {
		checkPermission(new ListenerPermission(SecurityConstants.LISTENER_INSTANCE));
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
		} else if(proxy instanceof MultiMapProxy) {
			return new SecureMultimapProxy(node, (MultiMapProxy) proxy);
		} else if(proxy instanceof TopicProxy) {
			return new SecureTopicProxy(node, (TopicProxy) proxy);
		} else if(proxy instanceof IList) {
			return new SecureListProxy(node, (IList) proxy);
		} else if(proxy instanceof ISet) {
			return new SecureSetProxy(node, (ISet) proxy);
		} else if(proxy instanceof AtomicNumberProxy) {
			return new SecureAtomicNumberProxy(node, (AtomicNumberProxy) proxy);
		} else if(proxy instanceof LockProxy) {
			return new SecureLockProxy(node, (LockProxy) proxy);
		} else if(proxy instanceof CountDownLatchProxy) {
			return new SecureCountDownLatchProxy(node, (CountDownLatchProxy) proxy);
		} else if(proxy instanceof SemaphoreProxy) {
			return new SecureSemaphoreProxy(node, (SemaphoreProxy) proxy);
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
			p = new TopicPermission(actualName, action);
		} else if (name.startsWith(Prefix.AS_LIST)) {
			p = new ListPermission(actualName, action);
		} else if (name.startsWith(Prefix.MULTIMAP)) {
			p = new MultiMapPermission(actualName, action);
		} else if (name.startsWith(Prefix.SET)) {
			p = new SetPermission(actualName, action);
		} else if (name.startsWith(Prefix.ATOMIC_NUMBER)) {
			p = new AtomicNumberPermission(actualName, action);
		} else if (name.startsWith(Prefix.IDGEN)) {
		} else if (name.startsWith(Prefix.SEMAPHORE)) {
			p = new SemaphorePermission(actualName, action);
		} else if (name.startsWith(Prefix.COUNT_DOWN_LATCH)) {
			p = new CountDownLatchPermission(actualName, action);
//		} else if (name.equals("lock")) {
//			p = new LockPermission(actualName, action);
		}
		checkPermission(p);
	}
	
	private void checkPermission(Permission permission) {
		if (permission != null && node.securityContext != null) {
			node.securityContext.checkPermission(permission);
		}
	}

	public boolean containsInstanceProxy(String name) {
		return factory.containsInstanceProxy(name);
	}
	
	public boolean containsInstanceProxy(ProxyKey proxyKey) {
		return factory.containsInstanceProxy(proxyKey);
	}
	
	public boolean containsExecutorServiceProxy(String name) {
		return factory.containsExecutorServiceProxy(name);
	}
	
	public Node getNode() {
		return factory.getNode();
	}
	
	public int hashCode() {
        return factory.hashCode();
    }

    public boolean equals(Object o) {
        return factory.equals(o);
    }
}
