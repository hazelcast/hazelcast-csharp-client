using System;

namespace Hazelcast.Client
{
	public class TransactionClientProxy : Hazelcast.Core.Transaction
	{
		ProxyHelper proxyHelper;
	
	    public TransactionClientProxy(String name, OutThread outThread,  HazelcastClient client) {
	        proxyHelper = new ProxyHelper(name, outThread, null, client);
	    }
	
	    public void begin() {
	        proxyHelper.doOp<object>(ClusterOperation.TRANSACTION_BEGIN, null, null);
	    }
	
	    public void commit() {
	        proxyHelper.doOp<object>(ClusterOperation.TRANSACTION_COMMIT, null, null);
	        ClientThreadContext threadContext = ClientThreadContext.get();
	        threadContext.removeTransaction();
	    }
	
	    public int getStatus() {
	        return 0;
	    }
	
	    public void rollback() {
	        proxyHelper.doOp<object>(ClusterOperation.TRANSACTION_ROLLBACK, null, null);
	        ClientThreadContext threadContext = ClientThreadContext.get();
	        threadContext.removeTransaction();
	    }
	}
}

