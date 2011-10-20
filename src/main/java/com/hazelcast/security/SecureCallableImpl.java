package com.hazelcast.security;

import java.io.DataInput;
import java.io.DataOutput;
import java.io.IOException;
import java.security.Principal;
import java.security.PrivilegedExceptionAction;
import java.util.Set;
import java.util.concurrent.Callable;

import javax.security.auth.Subject;

import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.core.HazelcastInstanceAware;
import com.hazelcast.impl.Node;
import com.hazelcast.nio.DataSerializable;
import com.hazelcast.nio.SerializationHelper;

public final class SecureCallableImpl<V> implements SecureCallable<V>, DataSerializable {
	
	private transient Node node;
	private Subject subject;
	private Callable<V> callable;
	
	public SecureCallableImpl() {
		super();
	}
	
	public SecureCallableImpl(Subject subject, Callable<V> callable) {
		super();
		this.subject = subject;
		this.callable = callable;
	}

	public V call() throws Exception {
		if(node != null && node.securityContext != null) {
			return node.securityContext.doAsPrivileged(subject, new PrivilegedExceptionAction<V>() {
				public V run() throws Exception {
					return callable.call();
				}
			});
		} else {
			return callable.call();
		}
	}
	
	public Subject getSubject() {
		return subject;
	}

	@Override
	public String toString() {
		return "SecureCallable [subject=" + subject + ", callable=" + callable
				+ "]";
	}

	public void writeData(DataOutput out) throws IOException {
		SerializationHelper.writeObject(out, callable);
		boolean hasSubject = subject != null;
		out.writeBoolean(hasSubject);
		if(hasSubject) {
			final Set<Principal> principals = subject.getPrincipals();
			out.writeInt(principals.size());
			for (Principal principal : principals) {
				SerializationHelper.writeObject(out, principal);
			}
		}
	}

	public void readData(DataInput in) throws IOException {
		callable = (Callable) SerializationHelper.readObject(in);
		boolean hasSubject = in.readBoolean();
		if(hasSubject) {
			subject = new Subject();
			int size = in.readInt();
			final Set<Principal> principals = subject.getPrincipals();
			for (int i = 0; i < size; i++) {
				Principal principal = (Principal) SerializationHelper.readObject(in);
				principals.add(principal);
			}
		}
	}

	public void setHazelcastInstance(HazelcastInstance hazelcastInstance) {
		if(callable instanceof HazelcastInstanceAware) {
			((HazelcastInstanceAware) callable).setHazelcastInstance(hazelcastInstance);
		}
	}

	public Node getNode() {
		return node;
	}

	public void setNode(Node node) {
		this.node = node;
	}
}