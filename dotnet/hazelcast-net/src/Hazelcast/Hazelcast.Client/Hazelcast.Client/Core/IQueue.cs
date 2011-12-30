using System;
using System.Collections.Generic;

namespace Hazelcast.Client
{
	public interface IQueue<E>:ICollection<E>, System.Collections.Generic.ICollection<E>
	{
    	bool offer(E e);
    
    	void put(E e) ;
    
    	bool offer(E e, long timeout);
    
    	E take();
    
    	E poll(long timeout);
    
    	int remainingCapacity();
    
    	int drainTo(System.Collections.Generic.ICollection<E> collection);
    
    	int drainTo(System.Collections.Generic.ICollection<E> collection, int i);
    
    	E poll();
    
   	 	E element();
    
    	E peek();
	}
}

