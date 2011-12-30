
using System;
using System.Threading;
using Hazelcast.Client.IO;
using Hazelcast.Client.Impl;


namespace Hazelcast.Client
{
	public class ProxyHelper
	{
		String name;
		OutThread outThread;
		static long counter=0;
		public ProxyHelper (String name, OutThread outThread, ListenerManager listenerManager)
		{
			this.name = name;
			this.outThread = outThread;
		}
		
		public Object doOp(ClusterOperation op, Object key, Object val)
		{
			return this.doOp(op, key, val, 0);
		}
		
		public Object doOp(ClusterOperation op, Object key, Object val, long ttl)
		{
			Packet request = prepareRequest(op, key, val, ttl);
			Packet response = callAndGetResult(request);
			return getValue(response);
		}
		
		public Packet callAndGetResult(Packet packet)
		{
			Call call  = new Call(packet);
			return doCall(call);
		}
		
		public Object getValue(Packet response){
			//Console.WriteLine("Bytes: " );
			//printBytes(response.value);
			return IOUtil.toObject(response.value);
		}
		
		private static void printBytes (byte[] bytes)
		{
			Console.WriteLine("Size for is: " + bytes.Length);
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
		
		//public static long newCallId() {
		//	return Interlocked.Increment(ref counter);
    	//}
    }
		
}


