using System;
using System.Threading;
using Hazelcast.Client.IO;
using Hazelcast.Client.Impl;
using Hazelcast.Core;
using System.Collections.Generic;
using Hazelcast.Impl.Base;

namespace Hazelcast.Client
{
	public class MultiMapClientProxy<K,V>: IMultiMap<K,V> 
	{
		
		private String name;
		private ProxyHelper proxyHelper;
		private ListenerManager lManager;
		
		public MultiMapClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client)
		{
			this.name = name;
			this.proxyHelper = new ProxyHelper(name, outThread, lManager, client);
			this.lManager = listenerManager;
		}
		
		public InstanceType getInstanceType(){
			return InstanceType.MULTIMAP;
		}

	    public void destroy(){
			proxyHelper.destroy();
		}
	
	    public Object getId(){
			return name;
		}
		
		public string getName() {
			return name.Substring(Prefix.MULTIMAP.Length);
		}
		
		public void Lock(K key) {
			ProxyHelper.check(key);
	        doLock(ClusterOperation.CONCURRENT_MAP_LOCK, key, -1);
	    }
	
	    public bool tryLock(K key) {
	        ProxyHelper.check(key);
	        return (bool) doLock(ClusterOperation.CONCURRENT_MAP_LOCK, key, 0);
	    }
	
	    public bool tryLock(K key, long time) {
	        ProxyHelper.check(key);
	        return (bool) doLock(ClusterOperation.CONCURRENT_MAP_LOCK, key, time);
	    }
	
	    private Object doLock(ClusterOperation operation, Object key, long timeout) {
	        Packet request = proxyHelper.prepareRequest(operation, key, null, 0);
			request.timeout = timeout;
	        Packet response = proxyHelper.callAndGetResult(request);
	        return proxyHelper.getValue(response);
	    }
	
	    public void unlock(K key) {
	        ProxyHelper.check(key);
	        proxyHelper.doOp<Object>(ClusterOperation.CONCURRENT_MAP_UNLOCK, key, null, 0);
	    }
	
	    public bool lockMap(long time) {
	        return (bool) doLock(ClusterOperation.CONCURRENT_MAP_LOCK_MAP, null, time);
	    }
	
	    public void unlockMap() {
	        doLock(ClusterOperation.CONCURRENT_MAP_UNLOCK_MAP, null, -1);
	    }
		
		public bool put(K key, V value) {
	        ProxyHelper.check(key);
	        ProxyHelper.check(value);
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_PUT_MULTI, key, value);
	    }
	
	    public System.Collections.Generic.ICollection<V> get(K key) {
	        ProxyHelper.check(key);
	        return proxyHelper.doOp<Values>(ClusterOperation.CONCURRENT_MAP_GET, key, null).getCollection<V>();
	    }
	
	    public bool remove(K key, V value) {
	        ProxyHelper.check(key);
	        ProxyHelper.check(value);
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_REMOVE_MULTI, key, value);
	    }
	
	    public System.Collections.Generic.ICollection<V> remove(K key) {
	        ProxyHelper.check(key);
	        return proxyHelper.doOp<Values>(ClusterOperation.CONCURRENT_MAP_REMOVE_MULTI, key, null).getCollection<V>();
	    }
		
		public System.Collections.Generic.ICollection<K> keySet() {
	        System.Collections.Generic.IList<K> list = proxyHelper.keys<K>(null);
	        return list;
	    }
		
		public bool containsKey(K key) {
	        ProxyHelper.check(key);
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_CONTAINS_KEY, key, null);
	    }
	
	    public bool containsValue(V value) {
	        ProxyHelper.check(value);
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_CONTAINS_VALUE, null, value);
	    }
	
	    public bool containsEntry(K key, V value) {
	        ProxyHelper.check(key);
	        ProxyHelper.check(value);
	        return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_CONTAINS_VALUE, key, value);
	    }
	
	    public int size() {
	        return (int) proxyHelper.doOp<int>(ClusterOperation.CONCURRENT_MAP_SIZE, null, null);
	    }
	
	    public void clear() {
	        System.Collections.Generic.ICollection<K> keys = keySet();
	        foreach (K key in keys) {
	            remove(key);
	        }
	    }
		
		 public int valueCount(K key) {
	        ProxyHelper.check(key);
	        return (int) proxyHelper.doOp<int>(ClusterOperation.CONCURRENT_MAP_VALUE_COUNT, key, null);
	    }
		
		public void addEntryListener(EntryListener<K, V> listener, bool includeValue) {
	        addEntryListener(listener, default(K), includeValue);
	    }
	
	    public void addEntryListener(EntryListener<K, V> listener, K key, bool includeValue) {
	        ProxyHelper.check(listener);
	        bool noEntryListenerRegistered = entryListenerManager().noListenerRegistered(key, name, includeValue, proxyHelper);
	        if (noEntryListenerRegistered == null) {
	            proxyHelper.doOp<object>(ClusterOperation.REMOVE_LISTENER, key, null);
	            noEntryListenerRegistered = true;
	        }
	        if (noEntryListenerRegistered) {
	            Call c = entryListenerManager().createNewAddListenerCall(proxyHelper, key, includeValue);
	            proxyHelper.doCall(c);
	        }
	        entryListenerManager().registerListener(name, key, includeValue, listener);
	    }
	
	    public void removeEntryListener(EntryListener<K, V> listener) {
	        ProxyHelper.check(listener);
	        proxyHelper.doOp<object>(ClusterOperation.REMOVE_LISTENER, null, null);
	        entryListenerManager().removeListener(name, null, listener);
	    }
	
	    public void removeEntryListener(EntryListener<K, V> listener, K key) {
	        ProxyHelper.check(listener);
	        ProxyHelper.check(key);
	        proxyHelper.doOp<object>(ClusterOperation.REMOVE_LISTENER, key, null);
	        entryListenerManager().removeListener(name, key, listener);
	    }
	
	    private EntryListenerManager entryListenerManager() {
	        return lManager.getEntryListenerManager();
	    }
	}
}

