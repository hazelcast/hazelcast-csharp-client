using System;
using Hazelcast.Core;
using Hazelcast.Client.Impl;

namespace Hazelcast.Client
{
	public class AtomicNumberClientProxy: IAtomicNumber
	{
		private String name;
		private ProxyHelper proxyHelper;
		private ListenerManager lManager;
		
		public AtomicNumberClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client)
		{
			this.name = name;
			this.proxyHelper = new ProxyHelper(name, outThread, lManager, client);
			this.lManager = listenerManager;
		}
		
		public String getName(){
			return this.name.Substring(Prefix.ATOMIC_NUMBER.Length);
		}
		
		public long addAndGet(long delta) {
	        return proxyHelper.doOp<long>(ClusterOperation.ATOMIC_NUMBER_ADD_AND_GET, 0L, delta);
	    }
	
	    public bool compareAndSet(long expect, long update) {
	        return proxyHelper.doOp<bool>(ClusterOperation.ATOMIC_NUMBER_COMPARE_AND_SET, expect, update);
	    }
	
	    public long decrementAndGet() {
	        return addAndGet(-1L);
	    }
	
	    public long get() {
	        return getAndAdd(0L);
	    }
	
	    public long getAndAdd(long delta) {
	        return proxyHelper.doOp<long>(ClusterOperation.ATOMIC_NUMBER_GET_AND_ADD, 0L, delta);
	    }
	
	    public long getAndSet(long newValue) {
	        return proxyHelper.doOp<long>(ClusterOperation.ATOMIC_NUMBER_GET_AND_SET, 0L, newValue);
	    }
	
	    public long incrementAndGet() {
	        return addAndGet(1L);
	    }
	
	    public void set(long newValue) {
	        getAndSet(newValue);
	    }
	
	    public void destroy() {
	        proxyHelper.destroy();
	    }
	
	    public Object getId() {
	        return name;
	    }
	
	    public InstanceType getInstanceType() {
	        return InstanceType.ATOMIC_NUMBER;
	    }

	}
}

