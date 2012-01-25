using System;
using System.Threading;
using Hazelcast.Client.IO;
using Hazelcast.Client.Impl;
using Hazelcast.Core;
using System.Collections.Generic;
using Hazelcast.Impl.Base;
using Hazelcast.Impl;
namespace Hazelcast.Client
{
	public class MapClientProxy<K, V>: IMap<K,V>
	{
		private String name;
		private ProxyHelper proxyHelper;
		private ListenerManager lManager;
		
		public MapClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client)
		{
			this.name = name;
			this.proxyHelper = new ProxyHelper(name, outThread, lManager, client);
			this.lManager = listenerManager;
		}
		
		public InstanceType getInstanceType(){
			return InstanceType.MAP;
		}

	    public void destroy(){
			proxyHelper.destroy();
		}
	
	    public Object getId(){
			return name;
		}
		
		public string getName() {
			return name.Substring(Prefix.MAP.Length);
		}
		
		public V put(K key, V value){
			Object o = proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_PUT, key, value);
			if(o==null)
				return default(V);
			return (V)o;
		}
		
		public V put(K key, V value, long timeout){
			return proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_PUT, (K)key, value, timeout);
		}
		
		public V get(K key){
			return proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_GET, (K)key, null);
		}
		
		public V remove(Object key){
			return proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_REMOVE, (K)key, null);
		}
		public bool remove(Object arg0, Object arg1) {
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_REMOVE_IF_SAME, arg0, arg1);
    	}
		
		public void flush(){
			proxyHelper.doOp<object>(ClusterOperation.CONCURRENT_MAP_FLUSH, null, null);
		}
		
		public Object tryRemove(K key, long timeout){
			return proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_TRY_REMOVE, (K)key, null, timeout);	
		}
		
		public bool tryPut(K key, V value, long timeout){
			return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_TRY_PUT, (K)key, value, timeout);	
		}
		
		public int size() {
     	   return proxyHelper.doOp<int>(ClusterOperation.CONCURRENT_MAP_SIZE, null, null, -1);
    	}
		
		public void clear(){
			foreach (K key in Keys()){
				this.remove(key);
			}
		}
		
		public void putTransient(K key, V value, long ttl){
			proxyHelper.doOp<object>(ClusterOperation.CONCURRENT_MAP_PUT_TRANSIENT, key, value, ttl);	
		}
		
		public V putIfAbsent(K key, V value, long ttl){
			return proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_PUT_IF_ABSENT, key, value, ttl);		
		}
		
		public V putIfAbsent(K key, V value){
			return proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_PUT_IF_ABSENT, key, value);		
		}
		
		public V tryLockAndGet(K key, long time){
			return proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_TRY_LOCK_AND_GET, key, null, time);		
		}
		
		public void putAndUnlock(K key, V value){
			proxyHelper.doOp<object>(ClusterOperation.CONCURRENT_MAP_PUT_AND_UNLOCK, key, value);			
		}
		
		public bool containsKey(Object arg0) {
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_CONTAINS_KEY, arg0, null, -1);
	    }
	
	    public bool containsValue(Object arg0) {
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_CONTAINS_VALUE, null, arg0, -1);
	    }
		
		public void Lock(K key){
			proxyHelper.doOp<object>(ClusterOperation.CONCURRENT_MAP_LOCK, key, null);			
		}

    	public bool tryLock(K key){
			return (bool)doLock(ClusterOperation.CONCURRENT_MAP_LOCK, key, 0);
		}

    	public bool tryLock(K key, long time){
			return (bool)doLock(ClusterOperation.CONCURRENT_MAP_LOCK, key, time);			
		}

    	public void unlock(K key){
			proxyHelper.doOp<object>(ClusterOperation.CONCURRENT_MAP_UNLOCK, key, null);
		}

    	public bool lockMap(long time){
			return (bool)doLock(ClusterOperation.CONCURRENT_MAP_LOCK_MAP, null, time);
		}

    	public void unlockMap(){
			proxyHelper.doOp<object>(ClusterOperation.CONCURRENT_MAP_UNLOCK_MAP, null, null);
		}
		
		public V replace(K arg0, V arg1) {
	        return proxyHelper.doOp<V>(ClusterOperation.CONCURRENT_MAP_REPLACE_IF_NOT_NULL, arg0, arg1);
	    }
	
	    public bool replace(K arg0, V arg1, V arg2) {
	        Hazelcast.Impl.Keys keys = new Hazelcast.Impl.Keys();
			keys.Add(new Hazelcast.IO.Data(IOUtil.toByte(arg1)));
			keys.Add(new Hazelcast.IO.Data(IOUtil.toByte(arg2)));
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_REPLACE_IF_SAME, arg0, keys);
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
			proxyHelper.doOp<object>(ClusterOperation.REMOVE_LISTENER, null, null);
			listenerManager().removeListener(name, null, listener);
		}
		
		public void removeEntryListener(EntryListener<K, V> listener, K key){
			proxyHelper.doOp<object>(ClusterOperation.REMOVE_LISTENER, key, null);
			listenerManager().removeListener(name, key, listener);
		}
		
		
		public MapEntry<K, V> getMapEntry(K key){
			CMapEntry cMapEntry = proxyHelper.doOp<CMapEntry>(ClusterOperation.CONCURRENT_MAP_GET_MAP_ENTRY, key, null);
        	if (cMapEntry == null) 
            	return null;
        	
        	return new ClientMapEntry<K,V>(cMapEntry, key, this);
		}
		
		public bool evict(object key){
			return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_EVICT, key, null);
		}
		
		public void addIndex(string attribute, bool ordered){
			proxyHelper.doOp<object>(ClusterOperation.ADD_INDEX, attribute, ordered);
		}

		public Dictionary<K, V> getAll(HashSet<K> keys){
			Hazelcast.Impl.Keys keyCollection = new Hazelcast.Impl.Keys();
			foreach(K key in keys ){
				byte[] bytes = IOUtil.toByte(key);
				keyCollection.Add(new Hazelcast.IO.Data(bytes));
			}
			Pairs pairs = proxyHelper.doOp<Pairs>(ClusterOperation.CONCURRENT_MAP_GET_ALL, keyCollection, null);
			List<KeyValue> list = pairs.lsKeyValues;
			Dictionary<K,V> dictionary = new Dictionary<K, V>();
			foreach(KeyValue k in list)
				dictionary.Add ((K)IOUtil.toObject(k.key.Buffer), (V)IOUtil.toObject(k.value.Buffer));				
			return dictionary;
		}
		
		public void putAll(Dictionary<K, V> map) {
	        Pairs pairs = new Pairs();
	        foreach (K key in map.Keys) {
	            V value = map[key];
	            pairs.addKeyValue(new KeyValue(IOUtil.toData(key), IOUtil.toData(value)));
	        }
	        proxyHelper.doOp<object>(ClusterOperation.CONCURRENT_MAP_PUT_ALL, null, pairs);
    	}
		
		public System.Collections.Generic.ICollection<V> Values(){
			return Values (null);
		}
		
		public System.Collections.Generic.ICollection<K> Keys(){
			return Keys (null);
		}
		
		public System.Collections.Generic.ICollection<V> Values(Hazelcast.Query.Predicate predicate){
			IDictionary<K,V> dictionary = entrySet(predicate);
			return dictionary.Values;
		}
		
		public System.Collections.Generic.ICollection<K> Keys(Hazelcast.Query.Predicate predicate){
			IDictionary<K,V> dictionary = entrySet(predicate);
			return dictionary.Keys;
		}
		
		public IDictionary<K, V> entrySet(Hazelcast.Query.Predicate predicate) {
        	System.Collections.Generic.IList<KeyValue> list = proxyHelper.entries<K>(predicate);
        	Dictionary<K,V> dic = new Dictionary<K,V>();
			foreach(KeyValue kv in list){
				dic.Add((K)IOUtil.toObject(kv.key.Buffer), (V)IOUtil.toObject(kv.value.Buffer));
			}
			return dic;
    	}
	}
}

