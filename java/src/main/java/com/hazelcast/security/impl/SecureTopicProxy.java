package com.hazelcast.security.impl;

import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.MessageListener;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.TopicProxy;
import com.hazelcast.impl.monitor.TopicOperationsCounter;
import com.hazelcast.monitor.LocalTopicStats;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.SecurityUtil;
import com.hazelcast.security.permission.TopicPermission;

final class SecureTopicProxy extends SecureProxySupport implements TopicProxy {
	
	private final TopicProxy proxy;
	final TopicPermission listenPermission;
	final TopicPermission publishPermission;
	final TopicPermission statsPermission;
	
	SecureTopicProxy(Node node, final TopicProxy proxy) {
		super(node);
		this.proxy = proxy;
		listenPermission = new TopicPermission(getName(), SecurityConstants.ACTION_LISTEN);
		publishPermission = new TopicPermission(getName(), SecurityConstants.ACTION_LISTEN);
		statsPermission = new TopicPermission(getName(), SecurityConstants.ACTION_STATISTICS);
	}
	
	private void checkListen() {
		SecurityUtil.checkPermission(node.securityContext, listenPermission);
	}
	
	public String getName() {
		return proxy.getName();
	}

	public void publish(Object message) {
		SecurityUtil.checkPermission(node.securityContext, publishPermission);
		proxy.publish(message);
	}

	public void addMessageListener(MessageListener listener) {
		checkListen();
		proxy.addMessageListener(listener);
	}

	public TopicOperationsCounter getTopicOperationCounter() {
		return proxy.getTopicOperationCounter();
	}

	public void removeMessageListener(MessageListener listener) {
		checkListen();
		proxy.removeMessageListener(listener);
	}

	public String getLongName() {
		return proxy.getLongName();
	}

	public LocalTopicStats getLocalTopicStats() {
		SecurityUtil.checkPermission(node.securityContext, statsPermission);
		return proxy.getLocalTopicStats();
	}

	public InstanceType getInstanceType() {
		return proxy.getInstanceType();
	}

	public void destroy() {
		SecurityUtil.checkPermission(node.securityContext, new TopicPermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public Object getId() {
		return proxy.getId();
	}
	
	public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
		proxy.setHazelcastInstance(hazelcastInstance);
	}
}
