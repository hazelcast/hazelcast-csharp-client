using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Client.IO;

namespace Hazelcast.Client
{
	public class QueueItemListenerManager
	{
		readonly private ConcurrentDictionary<String, List<ItemListener<Object>>> queueItemListeners = new ConcurrentDictionary<String, List<ItemListener<Object>>>();
		public QueueItemListenerManager ()
		{
		}
		
		public bool noListenerRegistered(String name) {
        	if (!queueItemListeners.ContainsKey(name)) {
            	return true;
        	}
        	return queueItemListeners[name].Count == 0;
    	}
		
		public void registerListener<E>(String name, ItemListener<E> listener) {
        	List<ItemListener<Object>> newListenersList = new List<ItemListener<Object>>();
        	List<ItemListener<Object>> listeners = queueItemListeners.GetOrAdd(name, newListenersList);
        	if (listeners == null) {
            	listeners = newListenersList;
        	}
        	listeners.Add(listener);
    	}
		
		public Call createNewAddItemListenerCall(ProxyHelper proxyHelper) {
        	Packet request = proxyHelper.createRequestPacket(ClusterOperation.ADD_LISTENER, null, null, -1);
			request.longValue = 1;
        	return proxyHelper.createCall(request);
    	}
		
		public void notifyListeners(Packet packet) 
		{
			Console.WriteLine("Notify is called");
	        List<ItemListener<Object>> list = null;
	        if (queueItemListeners.TryGetValue(packet.name, out list)) {
	            foreach (ItemListener<Object> listener in list) {
	                bool added = (bool) IOUtil.toObject(packet.value);
	                if (added) {
	                    listener.itemAdded<object>(new DataAwareItemEvent<object>(packet.name, ItemEventType.ADDED, new Hazelcast.IO.Data(packet.key)));
	                } else {
	                    listener.itemRemoved<object>(new DataAwareItemEvent<object>(packet.name, ItemEventType.ADDED, new Hazelcast.IO.Data(packet.key)));
	                }
	            }
	        }
    	}
		
		public void removeListener<E>(String name, ItemListener<E> listener) {
	        if (!queueItemListeners.ContainsKey(name)) {
	            return;
	        }
	        queueItemListeners[name].Remove(listener);
	        if (queueItemListeners[name].Count==0) {
				List<ItemListener<Object>> removed = null;
	            queueItemListeners.TryRemove(name, out removed);
	        }
    	}
	}
}

