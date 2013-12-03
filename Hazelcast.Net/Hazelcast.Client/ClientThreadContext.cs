using System;
using System.Threading;
using System.Collections.Concurrent;
using Hazelcast.Client.IO;

namespace Hazelcast.Client
{
	public class ClientThreadContext
	{
		private static ConcurrentDictionary<Thread, ClientThreadContext> mapContexts = new ConcurrentDictionary<Thread, ClientThreadContext>();
	    TransactionClientProxy transactionProxy;
	    ClientSerializer serializer = new ClientSerializer();
	    Thread thread;
	
	    public ClientThreadContext(Thread thread) {
	        this.thread = thread;
	    }
	
	    public static ClientThreadContext get() {
	        Thread currentThread = Thread.CurrentThread;
	        ClientThreadContext threadContext = null;
			
	        if (!mapContexts.TryGetValue(currentThread, out threadContext)) {
	            threadContext = mapContexts.GetOrAdd(currentThread, new ClientThreadContext(currentThread));
				foreach (Thread t in mapContexts.Keys){
					if(!t.IsAlive){
						ClientThreadContext o = null;
						mapContexts.TryRemove(t, out o);
					}
				}
	        }
	        return threadContext;
	    }
	
	    public static void shutdown() {
	        mapContexts.Clear();
	    }
	
	    public Hazelcast.Core.Transaction getTransaction(OutThread outThread,  HazelcastClient client) {
	        if (transactionProxy == null) {
	            transactionProxy = new TransactionClientProxy(null, outThread, client);
	        }
	        return transactionProxy;
	    }
	
	    public void removeTransaction() {
	        transactionProxy = null;
	    }
	
	    public byte[] toByte(Object obj) {
	        return serializer.toByte(obj);
	    }
	
	    public Object toObject(byte[] bytes) {
	        return serializer.toObject(bytes);
	    }
	}
}

