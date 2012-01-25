using System;
using Hazelcast.Core;

namespace Hazelcast.Client
{
	public class LockClientProxy :ILock
	{
		private Object lockObject;
		private ProxyHelper proxyHelper;
		
		public LockClientProxy (OutThread outThread, Object obj, HazelcastClient client) {
			this.proxyHelper = new ProxyHelper("", outThread, null, client);
        	this.lockObject = obj;
        	ProxyHelper.check(lockObject);
		}
		
		
		public Object getLockObject() {
	        return lockObject;
	    }
			
		public void Lock() {
	        doLock(-1);
	    }
	
	    public bool tryLock() {
	        return (bool) doLock(0);
	    }
	    
	    public bool tryLock(long time) {
	        return (bool) doLock(time);
	    }
	
	    public void unLock() {
	        proxyHelper.doOp<Object>(ClusterOperation.LOCK_UNLOCK, lockObject, null);
	    }
	    
	    private Object doLock(long timeout) {
	    	ClusterOperation operation = ClusterOperation.LOCK_LOCK;
	        Packet request = proxyHelper.prepareRequest(operation, lockObject, null, 0);
	        request.timeout = timeout;
	        Packet response = proxyHelper.callAndGetResult(request);
	        return proxyHelper.getValue(response);
	    }
		
		 public InstanceType getInstanceType() {
	        return InstanceType.LOCK;
	    }
	
	    public void destroy() {
	        proxyHelper.destroy();
	    }
	
	    public Object getId() {
	        return lockObject;
	    }
	}
}