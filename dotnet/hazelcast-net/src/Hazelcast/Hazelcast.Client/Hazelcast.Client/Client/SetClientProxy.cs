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
	public class SetClientProxy<E> : CollectionClientProxy<E>, Hazelcast.Client.ISet<E>
	{
		
		public SetClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client):base(outThread, name, listenerManager, client)
		{
			
		}
		
		public override String getName(){
			return name.Substring(Prefix.SET.Length);
		}
		
		
		
		public InstanceType getInstanceType(){
			return InstanceType.SET;
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
		
		
		void  System.Collections.Generic.ICollection<E>.Add(E e){
			Add (e);
		}
		
		public new virtual bool Add(E e){
			return proxyHelper.doOp<bool>(ClusterOperation.CONCURRENT_MAP_ADD_TO_SET, e, null);
		}
		
		
		public override System.Collections.Generic.IList<E> entries(){
			return proxyHelper.keys<E>(null);
		}
		
		
		public void ExceptWith (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public void IntersectWith (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public bool IsProperSubsetOf (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public bool IsProperSupersetOf (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public bool IsSubsetOf (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public bool IsSupersetOf (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public bool Overlaps (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public bool SetEquals (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public void SymmetricExceptWith (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		public void UnionWith (IEnumerable<E> other){
			throw new Exception("Not implemented!");
		}
		
	}
}

