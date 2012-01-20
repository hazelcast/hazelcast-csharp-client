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
	public abstract class CollectionClientProxy<E>: System.Collections.ObjectModel.Collection<E>, ICollection<E>, System.Collections.Generic.ICollection<E>
	{
		protected String name;
		protected ProxyHelper proxyHelper;
		protected ListenerManager lManager;
		
		
		public CollectionClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client)
		{
			this.name = name;
			this.proxyHelper = new ProxyHelper(name, outThread, lManager, client);
			this.lManager = listenerManager;
		}
		
	    public void destroy(){
			proxyHelper.destroy();			
		}
	
	    public Object getId(){
			return name;
		}
		
		public new int Count {
			get {
				return size();
			}
		}
		
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		
		/*public void CopyTo(E[] array, int arrayIndex){
			IEnumerator<E> enumerator = GetEnumerator();
			while(enumerator.MoveNext()){
				array[arrayIndex++] = enumerator.Current;
			}
		}*/
		
		public new IEnumerator<E> GetEnumerator(){
			System.Collections.Generic.IList<E> es = entries ();
			return es.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			System.Collections.Generic.IList<E> es = entries ();
				
			return ((IEnumerable)es).GetEnumerator();			  
		}
		
		public new virtual void Clear() {
        	System.Collections.Generic.IList<E> es = entries ();
            foreach(E e in es)
				Remove(e);
    	}
		
		public new virtual bool Contains(E e){
			IEnumerator<E> enumerator = GetEnumerator();
			bool found = false;
			while(enumerator.MoveNext()){
				found = enumerator.Current.Equals(e);
				if(found){
					break;
				}
			}	
			return found;
		}
		
		
		
		public abstract int size();
		
		public abstract System.Collections.Generic.IList<E> entries();
		
		public virtual bool Remove(E e) {
        	return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_REMOVE_ITEM, e, null);
    	}
		
		
		public abstract String getName();
		
		
		
		public abstract void addItemListener(ItemListener<E> listener, bool includeValue);
		
		public abstract void removeItemListener(ItemListener<E> listener);
	}
}

