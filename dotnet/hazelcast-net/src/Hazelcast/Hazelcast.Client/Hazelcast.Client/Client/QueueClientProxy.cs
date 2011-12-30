using System;
using System.Collections.Generic;
using System.Collections;
using Hazelcast.Client.Impl;
using Hazelcast.Core;
using Hazelcast.Client.IO;
using Hazelcast.IO;

namespace Hazelcast.Client
{
	public class QueueClientProxy<E>:IQueue<E>
	{
		private String name;
		private ProxyHelper proxyHelper;
		private ListenerManager lManager;
		
		
		public QueueClientProxy (OutThread outThread, String name, ListenerManager listenerManager)
		{
			this.name = name;
			this.proxyHelper = new ProxyHelper(name, outThread, lManager);
			this.lManager = listenerManager;
		}
		
		public int Count {
			get {
				return size();
			}
		}
		
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		
		public int size() {
        	return (int) proxyHelper.doOp(ClusterOperation.BLOCKING_QUEUE_SIZE, null, null);
    	}
		
		public String getName(){
			return name;
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
        	return (bool) proxyHelper.doOp(ClusterOperation.BLOCKING_QUEUE_OFFER, e, millis);
    	}
		private E innerPoll(long millis) {
        	return (E) proxyHelper.doOp(ClusterOperation.BLOCKING_QUEUE_POLL, null, millis);
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
			return (int) proxyHelper.doOp(ClusterOperation.BLOCKING_QUEUE_REMAINING_CAPACITY, null, null);	
		}
    
    	public bool Remove(E e){
			return (bool) proxyHelper.doOp(ClusterOperation.BLOCKING_QUEUE_REMOVE, null, e);
		}
		
		
    	public bool Contains(E e){
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
		
		public void Clear() {
        	while (poll() != null)
            	;
    	}
    
    	public E peek(){
			return (E) proxyHelper.doOp(ClusterOperation.BLOCKING_QUEUE_PEEK, null, null);
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

		List<E> entries ()
		{
			Object[] dItems = (Object[]) proxyHelper.doOp(ClusterOperation.BLOCKING_QUEUE_ENTRIES, null, null);
			     	List<E> entries = new List<E>();
			     	foreach (Object entry in dItems) {
			         	entries.Add((E)IOUtil.toObject(((Data)entry).Buffer));
			     	}
			return entries;
		}
		
		public IEnumerator<E> GetEnumerator(){
			List<E> es = entries ();
			return es.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			List<E> es = entries ();
				
			return ((IEnumerable)es).GetEnumerator();			  
		}
		
		public void CopyTo(E[] array, int arrayIndex){
			IEnumerator<E> enumerator = GetEnumerator();
			while(enumerator.MoveNext()){
				array[arrayIndex++] = enumerator.Current;
			}
		}
		
		public void addItemListener(ItemListener<E> listener, bool includeValue){
			lock(name){
				bool shouldCall = listenerManager().noListenerRegistered(name);
            	listenerManager().registerListener(name, listener);
            	if (shouldCall) {
                	Call c = listenerManager().createNewAddItemListenerCall(proxyHelper);
                	proxyHelper.doCall(c);
            	}
			}	
		}
			
		public void removeItemListener(ItemListener<E> listener){
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
		
	}
}

