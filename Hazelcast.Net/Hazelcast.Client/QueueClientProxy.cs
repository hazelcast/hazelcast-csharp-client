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
	public class QueueClientProxy<E>:CollectionClientProxy<E>, IQueue<E>
	{
		
		public QueueClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client):base(outThread, name, listenerManager, client)
		{
		}
		
		public override int size() {
        	return proxyHelper.doOp<int>(ClusterOperation.BLOCKING_QUEUE_SIZE, null, null);
    	}
		
		public override String getName(){
			return name.Substring(Prefix.QUEUE.Length);
		}
		
		public InstanceType getInstanceType(){
			return InstanceType.QUEUE;
		}
		
		
		public void Add(E e){
			
			innerOffer(e, 0);
		}
		
    	public bool offer(E e){
			return innerOffer(e, 0);
		}
    	public void put(E e) {
			innerOffer(e, -1);
		}
    
    	public bool offer(E e, long timeout){
			return innerOffer(e, timeout);
		}
		
		private bool innerOffer(E e, long millis) {
			checkNull(e);
        	return proxyHelper.doOp<bool>(ClusterOperation.BLOCKING_QUEUE_OFFER, e, millis);
    	}
		private E innerPoll(long millis) {
        	return proxyHelper.doOp<E>(ClusterOperation.BLOCKING_QUEUE_POLL, null, millis);
    	}
		
		public E take(){
			return innerPoll(-1);
		}
    
    	public E poll(){
			return (E) innerPoll(0);
		}
		
		public E poll(long timeout){
			return innerPoll(timeout);
		}
    
    	public int remainingCapacity(){
			return proxyHelper.doOp<int>(ClusterOperation.BLOCKING_QUEUE_REMAINING_CAPACITY, null, null);	
		}
    
    	public override bool Remove(E e){
			return proxyHelper.doOp<bool>(ClusterOperation.BLOCKING_QUEUE_REMOVE, null, e);
		}
	
    	public E Remove() {
        	E x = poll();
        	if (x != null)
            	return x;
        	else
        		throw new Exception("No such element!");
    	}
    
    	 public E element() {
        	E x = peek();
        	if (x != null)
            	return x;
        	else
            	throw new Exception("No such element!");
    	}	
		
		public override void Clear() {
        	while (poll() != null)
            	;
    	}
    
    	public E peek(){
			return proxyHelper.doOp<E>(ClusterOperation.BLOCKING_QUEUE_PEEK, null, null);
		}
		
		public int drainTo(System.Collections.Generic.ICollection<E> collection){
			return drainTo(collection, Int32.MaxValue);
		}
    
    	public int drainTo(System.Collections.Generic.ICollection<E> collection, int i){
			E e;
        	int counter = 0;
        	while (counter < i && (e = poll()) != null) {
            	collection.Add(e);
            	counter++;
        	}
        	return counter;
		}
		
		public bool addAll(System.Collections.Generic.ICollection<E> c) {
       		if (c == null)
            	throw new NullReferenceException();
        	if (c == this)
            	throw new Exception("Colllection is equal to this");
        	bool modified = false;
        	
        	foreach(E e in c){
            	if (offer(e))
                	modified = true;
        	}
        	return modified;
    	}

		public override System.Collections.Generic.IList<E> entries ()
		{
			Keys keys = proxyHelper.doOp<Keys>(ClusterOperation.BLOCKING_QUEUE_ENTRIES, null, null);
	     	
			List<E> list = new List<E>();
			for(int i=0;i<keys.Count();i++){
				list.Add((E)IOUtil.toObject(keys.Get(i).Buffer));
			}
			
			return list;
		}

		public override void addItemListener(ItemListener<Object> listener, bool includeValue){
			lock(name){
				bool shouldCall = listenerManager().noListenerRegistered(name);
            	listenerManager().registerListener(name, listener);
            	if (shouldCall) {
                	Call c = listenerManager().createNewAddItemListenerCall(proxyHelper);
                	proxyHelper.doCall(c);
            	}
			}	
		}
			
		public override void removeItemListener(ItemListener<Object> listener){
			lock(name){
				listenerManager().removeListener(name, listener);
            	Packet request = proxyHelper.createRequestPacket(ClusterOperation.REMOVE_LISTENER, null, null, -1);
            	Call c = proxyHelper.createCall(request);
            	proxyHelper.doCall(c);	
			}	
		}
		
		private QueueItemListenerManager listenerManager() {
        	return lManager.getQueueItemListenerManager();
    	}
		
		private void checkNull(E e){
			if(e == null){
				throw new NullReferenceException();
			}
		}
		
	}
}

