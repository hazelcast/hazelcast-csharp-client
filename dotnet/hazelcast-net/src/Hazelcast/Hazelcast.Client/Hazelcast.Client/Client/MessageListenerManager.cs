using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.Core;
using Hazelcast.Impl;

namespace Hazelcast.Client
{
	public class MessageListenerManager
	{
		readonly private ConcurrentDictionary<String, List<MessageListener<object>>> messageListeners = new ConcurrentDictionary<String, List<MessageListener<object>>>();

		public MessageListenerManager ()
		{
		}
		
		public bool noListenerRegistered(String name) {
	        if (!messageListeners.ContainsKey(name)) {
	            return true;
	        }
	        return messageListeners[name].Count == 0;
	    }
		
		public void registerListener<E>(String name, MessageListener<E> messageListener) {
	        List<MessageListener<object>> newListenersList = new List<MessageListener<object>>();
	        List<MessageListener<object>> listeners = messageListeners.GetOrAdd(name, newListenersList);
	        if (listeners == null) {
	            listeners = newListenersList;
	        }
	        listeners.Add(messageListener);
	    }
		
		public void removeListener<E>(String name, MessageListener<E> messageListener) {
	        if (!messageListeners.ContainsKey(name)) {
	            return;
	        }
	        messageListeners[name].Remove(messageListener);
	        if (messageListeners[name].Count==0) {
				List<MessageListener<object>> list = null;
	            messageListeners.TryRemove(name, out list);
	        }
	    }
		
		public Call createNewAddListenerCall(ProxyHelper proxyHelper) {
	        Packet request = proxyHelper.createRequestPacket(ClusterOperation.ADD_LISTENER, null, null, -1);
	        return proxyHelper.createCall(request);
	    }
		
		public void notifyListeners(Packet packet) {
	        List<MessageListener<object>> list;
	        if (messageListeners.TryGetValue(packet.name, out list)) {
	            foreach (MessageListener<object> messageListener in list) {
					messageListener.onMessage<object>(new DataMessage<object>(new Data(packet.key)));	
	            }
	        }
	    }
	}
}

