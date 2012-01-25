using System;
using System.Collections.Generic;
using System.Collections;
using Hazelcast.Client.Impl;
using Hazelcast.Core;
using Hazelcast.Client.IO;
using Hazelcast.IO;
using Hazelcast.Impl;
using Hazelcast.Impl.Base;


namespace Hazelcast.Client
{
	public class ListClientProxy<E> : CollectionClientProxy<E>, Hazelcast.Core.IList<E>
	{
		
		public ListClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client):base(outThread, name, listenerManager, client)
		{
			
		}
		
		public override String getName(){
			return name.Substring(Prefix.AS_LIST.Length);
		}
		
		public InstanceType getInstanceType(){
			return InstanceType.LIST;
		}
			
		public override void addItemListener(ItemListener<E> listener, bool includeValue){
			Call c = itemListenerManager().createNewAddListenerCall(proxyHelper, includeValue);
        	itemListenerManager().registerListener(name, listener);
        	proxyHelper.doCall(c);	
		}
			
		public override void removeItemListener(ItemListener<E> listener){
			
		}
		
		private ItemListenerManager itemListenerManager() {
        	return lManager.getItemListenerManager();
    	}
		
		public override bool Contains(E e){
        	return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_CONTAINS_KEY, e, null);
    	}
		
	
    	public override int size() {
        	return proxyHelper.doOp<int>(ClusterOperation.CONCURRENT_MAP_SIZE, null, null);
    	}
		
		
		public void Add(E e){
			proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_ADD_TO_LIST, e, null);
		}
		
		public override System.Collections.Generic.IList<E> entries(){
			System.Collections.Generic.IList<KeyValue> list = proxyHelper.entries<E>(null);
			List<E> l = new List<E>();
			foreach(KeyValue kv in list){
				Object o = IOUtil.toObject(kv.value.Buffer);
				l.Add((E)o);
			}
			return l;	
		}
		
	}
}

