using System;
using Hazelcast.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Client.IO;
using Hazelcast.Impl;
using System.Runtime.CompilerServices;
namespace Hazelcast.Client
{
	public class EntryListenerManager
	{
		private object NULL_KEY = new object();
		private ConcurrentDictionary<string, ConcurrentDictionary<object, ConcurrentBag<EntryListenerHolder>>> entryListeners =
            new ConcurrentDictionary<string, ConcurrentDictionary<object, ConcurrentBag<EntryListenerHolder>>>();
		
		public EntryListenerManager ()
		{
		}
		
		public void notifyListeners(Packet packet) {
        	Object oldValue = null;
        	
			Object value = IOUtil.toObject(packet.value);
			if (value is Keys) {
            	Keys keys = (Keys) value;
            	if(keys.Count() > 0){
					value = IOUtil.toObject(keys.Get(0).Buffer);
					if(keys.Count() > 1){
						oldValue = IOUtil.toObject(keys.Get(1).Buffer);
					}	
				}
        	}
        	EntryEvent<object, object> e = new EntryEvent<object, object>(packet.name, null, (int) packet.longValue, IOUtil.toObject(packet.key), oldValue, value);
        	String name = packet.name;
        	Object key = toKey(e.Key);
			//Console.WriteLine("Name: " + name + " Key: " + key + " Value: " + value + "OldValue: " + oldValue) ;
        	if (entryListeners.ContainsKey(name)) {
           		notifyListeners(e, entryListeners[name][NULL_KEY]);
            	if (key != NULL_KEY) {
					if(entryListeners[name].ContainsKey(key)){
               		notifyListeners(e, entryListeners[name][key]);
					}
				}
        	}
    	}
		
		private void notifyListeners<K,V>(EntryEvent<K, V> e, ConcurrentBag<EntryListenerHolder> collection) {
        if (collection == null) {
            return;
        }
        EntryEvent<K, V> eventNoValue = e.Value != null ? 
                new EntryEvent<K, V>(e.Name, e.Member, (int)e.EntryEventType, e.Key, default(V), default(V)) :
                e;
        switch (e.EntryEventType) {
            case EntryEventType.ADDED:
                foreach (EntryListenerHolder<K,V> holder in collection) {
                    holder.listener.entryAdded(holder.includeValue ? e : eventNoValue);
                }
                break;
            case EntryEventType.UPDATED:
                foreach (EntryListenerHolder<K,V> holder in collection) {
                    holder.listener.entryUpdated(holder.includeValue ? e : eventNoValue);
                }
                break;
            case EntryEventType.REMOVED:
                foreach (EntryListenerHolder<K,V> holder in collection) {
                    holder.listener.entryRemoved(holder.includeValue ? e : eventNoValue);
                }
                break;
            case EntryEventType.EVICTED:
                foreach (EntryListenerHolder<K,V> holder in collection) {
                    holder.listener.entryEvicted(holder.includeValue ? e : eventNoValue);
                }
                break;
        }
    }
		public Call createNewAddListenerCall(ProxyHelper proxyHelper, Object key, bool includeValue){
			
			Packet request = proxyHelper.createRequestPacket(ClusterOperation.ADD_LISTENER, IOUtil.toByte(key), null, 0);
        	request.longValue = (includeValue ? 1 : 0);
        	return proxyHelper.createCall(request);
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void registerListener<K,V> (String name, Object key, bool includeValue, EntryListener<K,V> listener) { 
			
			ConcurrentDictionary<object, ConcurrentBag<EntryListenerHolder>> map = null;
			entryListeners.TryGetValue(name, out map);
	        key = toKey(key);
	        if (map==null) {
	            map = new ConcurrentDictionary<object, ConcurrentBag<EntryListenerHolder>>();
	            ConcurrentDictionary<object, ConcurrentBag<EntryListenerHolder>> map2 = entryListeners.GetOrAdd(name, map);
	            if (map2 != null) {
	                map = map2;
	            }
	        } 
			if (!map.ContainsKey(key)) {
	            map.GetOrAdd(key, new ConcurrentBag<EntryListenerHolder>());
	        }
	        map[key].Add(new EntryListenerHolder<K,V>(listener, includeValue));	
				
		}
		public void removeListener<K,V> (String name, Object key, EntryListener<K,V> listener) { 
			
		}
		
		public Boolean noListenerRegistered(Object key, String name, bool includeValue, ProxyHelper proxyHelper){
			
			ConcurrentDictionary<object, ConcurrentBag<EntryListenerHolder>> map = null;
			entryListeners.TryGetValue(name, out map);
        	key = toKey(key);
        	if (map == null || !map.ContainsKey(key)) {
            	return true;
        	}
        	foreach (EntryListenerHolder holder in map[key]) {
            	if (holder.includeValue == includeValue) {
            		return false;
            	} else if (includeValue) {
                	proxyHelper.doOp<object>(ClusterOperation.REMOVE_LISTENER, key, null);
            	}
        	}
        	return true;
		}
		
		private object toKey(object key) {
        	return key != null ? key : NULL_KEY;
		}
	}
	
	abstract class EntryListenerHolder {
		//public abstract Hazelcast.Core.EntryListener<object,object> listener{get; set;}
        public bool includeValue{get; set;}
	}
	
	class EntryListenerHolder<K,V>: EntryListenerHolder {
        public Hazelcast.Core.EntryListener<K,V> listener {get; set;}
        //public bool includeValue {get; set;}

        public EntryListenerHolder(Hazelcast.Core.EntryListener<K,V> listener, bool includeValue) {
            this.listener = listener;
            this.includeValue = includeValue;
        }
    }
}

