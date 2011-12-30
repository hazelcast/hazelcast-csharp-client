using System;
using Hazelcast.Client;
using Hazelcast.Core;
using System.Threading;
using System.Collections.Concurrent;

namespace Hazelcast.Client.Impl
{
	public class ListenerManager
	{
		BlockingCollection<Object> queue = new BlockingCollection<Object>(1000);
		private EntryListenerManager entryListenerManager;
		private QueueItemListenerManager queueItemListenerManager;
		private MessageListenerManager messageListenerManager;
		public ListenerManager ()
		{
			entryListenerManager = new EntryListenerManager();
			queueItemListenerManager = new QueueItemListenerManager();
			messageListenerManager = new MessageListenerManager();
		}
		
		public void enQueue(Object o)
		{
			queue.Add(o);
		}
		
		public void run(){
			while(true){
				Object obj;
            	if (!queue.TryTake(out obj, 100)) {
               		return;
            	}
            	if (obj is Packet) {
                	Packet packet = (Packet) obj;
					if(getInstanceType(packet.name).Equals(InstanceType.MAP)){
                		entryListenerManager.notifyListeners(packet);
					}else if(getInstanceType(packet.name).Equals(InstanceType.QUEUE)){
						queueItemListenerManager.notifyListeners(packet);			
					}else if(getInstanceType(packet.name).Equals(InstanceType.TOPIC)){
						messageListenerManager.notifyListeners(packet);
					} 
            	}
			}
		}
		
		public static InstanceType getInstanceType(String name) 
		{
	        if (name.StartsWith(Prefix.ATOMIC_NUMBER)) {
	            return InstanceType.ATOMIC_NUMBER;
	        } else if (name.StartsWith(Prefix.COUNT_DOWN_LATCH)) {
	            return InstanceType.COUNT_DOWN_LATCH;
	        } else if (name.StartsWith(Prefix.IDGEN)) {
	            return InstanceType.ID_GENERATOR;
	        } else if (name.StartsWith(Prefix.AS_LIST)) {
	            return InstanceType.LIST;
	        } else if (name.StartsWith(Prefix.MAP)) {
	            return InstanceType.MAP;
	        } else if (name.StartsWith(Prefix.MULTIMAP)) {
	            return InstanceType.MULTIMAP;
	        } else if (name.StartsWith(Prefix.QUEUE)) {
	            return InstanceType.QUEUE;
	        } else if (name.StartsWith(Prefix.SEMAPHORE)) {
	            return InstanceType.SEMAPHORE;
	        } else if (name.StartsWith(Prefix.SET)) {
	            return InstanceType.SET;
	        } else if (name.StartsWith(Prefix.TOPIC)) {
	            return InstanceType.TOPIC;
	        } else {
	            throw new Exception("Unknown InstanceType " + name);
	        }
    }
		
		
		
		public EntryListenerManager getEntryListenerManager(){
			return entryListenerManager;
		}
		public QueueItemListenerManager getQueueItemListenerManager(){
			return queueItemListenerManager;
		}
		public MessageListenerManager getMessageListenerManager(){
			return messageListenerManager;
		}
		
		public static ListenerManager start ()
		{
			ListenerManager listenerManager = new ListenerManager();
			Thread thread = new Thread (new ThreadStart (listenerManager.run));
			thread.Start ();
			return listenerManager;
		}
	}
}

