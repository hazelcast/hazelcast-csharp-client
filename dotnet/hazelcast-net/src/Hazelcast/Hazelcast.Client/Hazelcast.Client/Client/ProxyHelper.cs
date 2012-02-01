
using System;
using System.Threading;
using System.Collections.Generic;
using Hazelcast.Client.IO;
using Hazelcast.Client.Impl;
using Hazelcast.Impl;
using Hazelcast.Impl.Base;


namespace Hazelcast.Client
{
	public class ProxyHelper
	{
		static long counter=0;
		
		String name;
		OutThread outThread;
		HazelcastClient client;
		
		public ProxyHelper (String name, OutThread outThread, ListenerManager listenerManager, HazelcastClient client)
		{
			this.name = name;
			this.outThread = outThread;
			this.client = client;
		}
		
		public V doOp<V>(ClusterOperation op, Object key, Object val)
		{
			Object o = this.doOp<V>(op, key, val, 0);
			if(o==null)
				return default(V);
			return (V)o;
		}
		
		public V doOp<V>(ClusterOperation op, Object key, Object val, long ttl)
		{
			Packet request = prepareRequest(op, key, val, ttl);
			Packet response = callAndGetResult(request);
			Object o = getValue(response);
			if(o==null)
				return default(V);
			return (V)o;
		}
		
		public Packet callAndGetResult(Packet packet)
		{
			Call call  = new Call(packet);
			return doCall(call);
		}
		
		public Object getValue(Packet packet){
			Object response = IOUtil.toObject(packet.value);
			if(response is ClientServiceException){
				Console.WriteLine("Exception is: " + ((ClientServiceException)response).Exception);
				throw ((ClientServiceException)response).Exception;
			}
			return response;
		}
		
		public void destroy() {
        	doOp<object>(ClusterOperation.DESTROY, null, null);
       	 	client.destroy(name);
    	}
		
		private static void printBytes (byte[] bytes)
		{
			foreach (byte b in bytes) {
				Console.Write (b);
				Console.Write (".");
			}
			
		}
		
		
		public Packet prepareRequest(ClusterOperation operation, Object key, Object val, long ttl) {
	        byte[] k = null;
	        byte[] v = null;
	        if (key != null) {
	            k = IOUtil.toByte(key);
	        }
	        if (val != null) {
	            v = IOUtil.toByte(val);
	        }
	        Packet packet = createRequestPacket(operation, k, v, ttl);
	        return packet;
    	}
		
		 public Packet createRequestPacket(ClusterOperation operation, byte[] key, byte[] val, long ttl) {
	        Packet request = new Packet();
			request.name = name;
	        request.operation = (byte)operation;
			request.key = key;
	        request.value = val;
			request.threadId = Thread.CurrentThread.ManagedThreadId;
	        if (ttl > 0) {
	            request.timeout = ttl;
	        }
	        return request;
    	}
		
		public Packet doCall(Call call){
			outThread.enQueue(call);
			Packet response = call.getResult();
			return response;
			
		}
		
		public Call createCall(Packet request) {
        	//long id = newCallId();
			return new Call(request);
        }
		
		public void doFireAndForget(ClusterOperation operation, Object key, Object value) {
	        Packet request = prepareRequest(operation, key, value, 0);
	        Call fireNForgetCall = createCall(request);
	        fireNForgetCall.FireNforget = true;
	        outThread.enQueue(fireNForgetCall);
	    }
		
		public System.Collections.Generic.IList<KeyValue> entries<E>(Hazelcast.Query.Predicate predicate) {
 			Keys keys = doOp<Keys>(ClusterOperation.CONCURRENT_MAP_ITERATE_ENTRIES, default(E), predicate);
			List<KeyValue> list = new List<KeyValue>();
			for(int i=0;i<keys.Count();i++){
				KeyValue kv = (KeyValue)IOUtil.toObject(keys.Get(i).Buffer);
				list.Add(kv);
			}
			return list;	
		}
		
		public System.Collections.Generic.IList<E> keys<E>(Hazelcast.Query.Predicate predicate) {
	        Keys keys =  doOp<Keys>(ClusterOperation.CONCURRENT_MAP_ITERATE_KEYS, default(E), predicate);
			List<E> list = new List<E>();
	        for (int i=0;i<keys.Count();i++){
				list.Add((E)IOUtil.toObject(keys.Get(i).Buffer));
			}
			
	        return list;
    	}
		
		public static void check(Object obj) {
	        if (obj == null) {
	            throw new NullReferenceException("Object cannot be null.");
	        }
    	}
		
		//public static long newCallId() {
		//	return Interlocked.Increment(ref counter);
    	//}
    }
		
}


