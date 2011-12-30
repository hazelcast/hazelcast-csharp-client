using System;
using System.Threading;
using Hazelcast.Client.IO;
using Hazelcast.Client.Impl;
using Hazelcast.Core;
using System.Collections.Generic;
using Hazelcast.Impl.Base;
namespace Hazelcast.Client
{
	public class MapClientProxy<K, V>: IMap<K,V>
	{
		private String name;
		private ProxyHelper proxyHelper;
		private ListenerManager lManager;
		
		public MapClientProxy (OutThread outThread, String name, ListenerManager listenerManager)
		{
			this.name = name;
			this.proxyHelper = new ProxyHelper(name, outThread, lManager);
			this.lManager = listenerManager;
		}
		
		public V put(K key, V value){
			return (V)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_PUT, (K)key, value);
		}
		
		public V put(K key, V value, long timeout){
			return (V)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_PUT, (K)key, value, timeout);
		}
		
		public V get(K key){
			return (V)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_GET, (K)key, null);
		}
		
		public V remove(Object key){
			return (V)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_REMOVE, (K)key, null);
		}
		
		public void flush(){
			proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_FLUSH, null, null);
		}
		
		public string getName() {
			return name.Substring(2);
		}
		
		public Object tryRemove(K key, long timeout){
			return (V)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_TRY_REMOVE, (K)key, null, timeout);	
		}
		
		public bool tryPut(K key, V value, long timeout){
			return (bool)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_TRY_PUT, (K)key, value, timeout);	
		}
		
		public int size() {
     	   return (int) proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_SIZE, null, null, -1);
    	}
		
		public void putTransient(K key, V value, long ttl){
			proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_PUT_TRANSIENT, key, value, ttl);	
		}
		public V putIfAbsent(K key, V value, long ttl){
			return (V)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_PUT_IF_ABSENT, key, value, ttl);		
		}
		public V putIfAbsent(K key, V value){
			return (V)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_PUT_IF_ABSENT, key, value);		
		}
		public V tryLockAndGet(K key, long time){
			return (V)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_TRY_LOCK_AND_GET, key, null, time);		
		}
		public void putAndUnlock(K key, V value){
			proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_PUT_AND_UNLOCK, key, value);			
		}
		public bool containsKey(Object arg0) {
	        return (bool) proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_CONTAINS_KEY, arg0, null, -1);
	    }
	
	    public bool containsValue(Object arg0) {
	        return (bool) proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_CONTAINS_VALUE, null, arg0, -1);
	    }
		
		public void Lock(K key){
			proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_LOCK, key, null);			
		}

    	public bool tryLock(K key){
			return (bool)doLock(ClusterOperation.CONCURRENT_MAP_LOCK, key, 0);
		}

    	public bool tryLock(K key, long time){
			return (bool)doLock(ClusterOperation.CONCURRENT_MAP_LOCK, key, time);			
		}

    	public void unlock(K key){
			proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_UNLOCK, key, null);
		}

    	public bool lockMap(long time){
			return (bool)doLock(ClusterOperation.CONCURRENT_MAP_LOCK_MAP, null, time);
		}

    	public void unlockMap(){
			proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_UNLOCK_MAP, null, null);
		}
		
		public V replace(K arg0, V arg1) {
	        return (V) proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_REPLACE_IF_NOT_NULL, arg0, arg1);
	    }
	
	    public bool replace(K arg0, V arg1, V arg2) {
	        Object[] arr = new Object[2];
	        arr[0] = arg1;
	        arr[1] = arg2;
	        return (bool) proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_REPLACE_IF_SAME, arg0, arr);
	    }
		
		private Object doLock(ClusterOperation operation, Object key, long timeout) {
        	Packet request = proxyHelper.prepareRequest(operation, key, null, 0);

        	request.timeout = timeout;
        	Packet response = proxyHelper.callAndGetResult(request);
        	return proxyHelper.getValue(response);
    	}	
		
		public void addEntryListener(EntryListener<K, V> listener, bool includeValue){
			addEntryListener(listener, default(K), includeValue);	
		}
		
		public void addEntryListener(EntryListener<K, V> listener, K key, bool includeValue) {
        
        	bool noEntryListenerRegistered = listenerManager().noListenerRegistered(key, name, includeValue, proxyHelper);
        	if (noEntryListenerRegistered) {
           	 	Call c = listenerManager().createNewAddListenerCall(proxyHelper, key, includeValue);
           	 	proxyHelper.doCall(c);
        	}
        	listenerManager().registerListener(name, key, includeValue, listener);
    	}
		
		
		private EntryListenerManager listenerManager() {
        	return lManager.getEntryListenerManager();
    	}



		public void removeEntryListener(EntryListener<K, V> listener){
			proxyHelper.doOp(ClusterOperation.REMOVE_LISTENER, null, null);
			listenerManager().removeListener(name, null, listener);
		}
		public void removeEntryListener(EntryListener<K, V> listener, K key){
			proxyHelper.doOp(ClusterOperation.REMOVE_LISTENER, key, null);
			listenerManager().removeListener(name, key, listener);
		}
		//a bit hard to implement
		public MapEntry<K, V> getMapEntry(K key){
			return null;
		}
		
		public bool evict(object key){
			return (bool)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_EVICT, key, null);
		}
		
		public void addIndex(string attribute, bool ordered){
			proxyHelper.doOp(ClusterOperation.ADD_INDEX, attribute, ordered);
		}

		public Dictionary<K, V> getAll(HashSet<K> keys){
			Hazelcast.Impl.Keys keyCollection = new Hazelcast.Impl.Keys();
			foreach(K key in keys ){
				byte[] bytes = IOUtil.toByte(key);
				
				keyCollection.Add(new Hazelcast.IO.Data(bytes));
			}
			Pairs pairs = (Pairs)proxyHelper.doOp(ClusterOperation.CONCURRENT_MAP_GET_ALL, keyCollection, null);
			List<KeyValue> list = pairs.lsKeyValues;
			Dictionary<K,V> dictionary = new Dictionary<K, V>();
			foreach(KeyValue k in list){
				dictionary.Add ((K)IOUtil.toObject(k.key.Buffer), (V)IOUtil.toObject(k.value.Buffer));				
			}
			return dictionary;
		}


		
		
		
		private static void printBytes (byte[] bytes)
		{
			Console.WriteLine("Size for is: " + bytes.Length);
			foreach (byte b in bytes) {
				Console.Write (b);
				Console.Write (".");
			}
			
		}
	}
}

