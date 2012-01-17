using System;
using System.Collections.Generic;
using System.Collections;
using Hazelcast.Client.Impl;
using Hazelcast.Core;
using Hazelcast.Client.IO;
using Hazelcast.IO;
using Hazelcast.Impl;


namespace Hazelcast.Client
{
	public class ListClientProxy<E> : CollectionClientProxy<E>, IList<E>
	{
		
		public ListClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client):base(outThread, name, listenerManager, client)
		{
			
		}
		
			
		public override void addItemListener(ItemListener<E> listener, bool includeValue){
			
		}
			
		public override void removeItemListener(ItemListener<E> listener){
			
		}
		
		public override bool Contains(E e){
        	return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_CONTAINS_KEY, e, null);
    	}
		
	
    	public override int size() {
        	return proxyHelper.doOp<int>(ClusterOperation.CONCURRENT_MAP_SIZE, null, null);
    	}
		
		
		public override void Add(E e){
			proxyHelper.doOp<object>(ClusterOperation.CONCURRENT_MAP_ADD_TO_LIST, e, null);
		}
		
		public override System.Collections.Generic.IList<E> entries(){
			return proxyHelper.entries<E>(null);
		}
		
	}
}

