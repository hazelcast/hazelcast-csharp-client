using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Hazelcast.Core;
namespace Hazelcast.Client
{
	public class ItemListenerManager
	{
		ConcurrentDictionary<ItemListener<Object>, EntryListener<Object, Object>> itemListener2EntryListener = new ConcurrentDictionary<ItemListener<Object>, EntryListener<Object, Object>>();
		private EntryListenerManager entryListenerManager;

	    public ItemListenerManager(EntryListenerManager entryListenerManager) {
	        this.entryListenerManager = entryListenerManager;
	    }
	
	    public void registerListener<E>(String name, ItemListener<E> itemListener) {
	        lock(itemListener2EntryListener){
				EntryListener<Object, Object> e = new EntryAdapter<Object, Object>(itemListener, name);
		        entryListenerManager.registerListener<Object,Object>(name, null, true, e);
		        itemListener2EntryListener[itemListener]= e;
			}
	    }
	
	    public void removeListener<E>(String name, ItemListener<E> itemListener) {
	        EntryListener<Object,Object> entryListener;
			itemListener2EntryListener.TryRemove(itemListener, out entryListener);
	        entryListenerManager.removeListener<Object,Object>(name, null, entryListener);
	    }
	
	    public Call createNewAddListenerCall(ProxyHelper proxyHelper, bool includeValue) {
	        Packet request = proxyHelper.createRequestPacket(ClusterOperation.ADD_LISTENER, null, null, 0);
	        request.longValue = 0;
	        return proxyHelper.createCall(request);
	    }
	
	    public System.Collections.Generic.ICollection<Call> calls(HazelcastClient client) {
	        return new List<Call>();
	    }
	}
	
	public class EntryAdapter<K,V> : EntryListener<K,V>
	{
		ItemListener<V> listener;
		String name;
		public EntryAdapter(ItemListener<V> itemListener, String name){
			this.listener = itemListener;
			this.name = name;
		}
		
		public void entryAdded(EntryEvent<K, V> e) {
            //Console.WriteLine("EEE" + e);
            //listener.itemAdded<object>(new DataAwareItemEvent<object>(name, ItemEventType.ADDED, e.Value));
			//itemListener.itemAdded(new DataAwareItemEvent(name, ItemEventType.ADDED, dataAwareEntryEvent.getNewValueData()));
		}
		
		public void entryRemoved(EntryEvent<K, V> e) {
			//Console.WriteLine("EEE" + e);
			//
				//DataAwareEntryEvent dataAwareEntryEvent = (DataAwareEntryEvent) e;	
			//itemListener.itemAdded(new DataAwareItemEvent(name, ItemEventType.REMOVED, dataAwareEntryEvent.getNewValueData()));
		}
		
		public void entryUpdated(EntryEvent<K, V> e){
			
		}
		
		public void entryEvicted(EntryEvent<K, V> e){
			
		}
	}
}

