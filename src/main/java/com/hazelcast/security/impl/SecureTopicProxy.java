package com.hazelcast.security.impl;

import com.hazelcast.core.MessageListener;
import com.hazelcast.impl.Node;
import com.hazelcast.impl.TopicProxy;
import com.hazelcast.impl.monitor.TopicOperationsCounter;
import com.hazelcast.monitor.LocalTopicStats;
import com.hazelcast.security.SecurityConstants;
import com.hazelcast.security.permission.TopicPermission;

final class SecureTopicProxy extends SecureProxySupport implements TopicProxy {
	
	private final TopicProxy proxy;
	
	SecureTopicProxy(Node node, final TopicProxy proxy) {
		super(node);
		this.proxy = proxy;
	}
	
	private void checkListen() {
		checkPermission(new TopicPermission(getName(), SecurityConstants.ACTION_LISTEN));
	}
	
	public String getName() {
		return proxy.getName();
	}

	public void publish(Object message) {
		checkPermission(new TopicPermission(getName(), SecurityConstants.ACTION_PUBLISH));
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
		checkPermission(new TopicPermission(getName(), SecurityConstants.ACTION_STATISTICS));
		return proxy.getLocalTopicStats();
	}

	public InstanceType getInstanceType() {
		return proxy.getInstanceType();
	}

	public void destroy() {
		checkPermission(new TopicPermission(getName(), SecurityConstants.ACTION_DESTROY));
		proxy.destroy();
	}

	public Object getId() {
		return proxy.getId();
	}
}
