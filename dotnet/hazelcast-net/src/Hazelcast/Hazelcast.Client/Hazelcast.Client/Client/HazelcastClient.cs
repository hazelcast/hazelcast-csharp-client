using System;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Impl;
namespace Hazelcast.Client
{
	public class HazelcastClient
	{
		private readonly OutThread outThread;		
		private readonly InThread inThread;
		private readonly ListenerManager listenerManager;
		ConcurrentDictionary<long, Call> calls = new ConcurrentDictionary<long, Call>();
		Dictionary<Object, Object> proxies = new Dictionary<Object, Object>();
		
		private HazelcastClient (String groupName, String groupPass, String address)
		{
			TcpClient tcp = new TcpClient(address, 5701);
			tcp.GetStream().Write(Packet.HEADER, 0, Packet.HEADER.Length);
			this.listenerManager = ListenerManager.start();
			this.outThread = OutThread.start(tcp, calls);
			this.inThread = InThread.start(tcp, calls, listenerManager);
			
			
		}
		
		public static HazelcastClient newHazelcastClient(String groupName, String groupPass, String address){
			return new HazelcastClient(groupName, groupPass, address);
		}
		
		public IMap<K,V> getMap<K,V>(String name){
			return (IMap<K,V>)getClientProxy(Prefix.MAP + name, ()=> new MapClientProxy<K, V>(outThread, Prefix.MAP + name, listenerManager));
		}
		
		public IQueue<E> getQueue<E>(String name){
			return (IQueue<E>)getClientProxy(Prefix.QUEUE + name, () => new QueueClientProxy<E>(outThread, Prefix.QUEUE +name, listenerManager));
		}
		
		public ITopic<E> getTopic<E>(String name){
			return (ITopic<E>)getClientProxy(Prefix.TOPIC + name, () => new TopicClientProxy<E>(outThread, Prefix.TOPIC +name, listenerManager));
		}
		
		private Object getClientProxy(Object o, Func<Object> func)
		{
			Object proxy=null;
        	if (!proxies.TryGetValue(o,out proxy)) {
				lock (proxies) {
		            if (!proxies.TryGetValue(o,out proxy)) {
		            	proxy = func.Invoke();
		               	proxies.Add(o, proxy);
		            }
	        	}
        	}
        	return proxies[o];
		}
	}
}

