using System;
using Hazelcast.Client;
using Hazelcast.Core;
using System.Threading;
using System.Collections.Concurrent;

namespace Hazelcast.Client.Impl
{
	public class ListenerManager : ClientThread
	{
		BlockingCollection<Object> queue = new BlockingCollection<Object>(1000);
		private EntryListenerManager entryListenerManager;
		private QueueItemListenerManager queueItemListenerManager;
		private MessageListenerManager messageListenerManager;
		private ItemListenerManager itemListenerManager;
		private MembershipListenerManager membershipListenerManager;
		private InstanceListenerManager instanceListenerManager;
		
		public ListenerManager (HazelcastClient client)
		{
			entryListenerManager = new EntryListenerManager();
			queueItemListenerManager = new QueueItemListenerManager();
			messageListenerManager = new MessageListenerManager();
			itemListenerManager = new ItemListenerManager(entryListenerManager);
			membershipListenerManager = new MembershipListenerManager(client);
			instanceListenerManager = new InstanceListenerManager(client);
		}
		
		public void enQueue(Object o)
		{
			queue.Add(o);
		}
		
		protected override void customRun(){
			Object obj;
        	if (!queue.TryTake(out obj, 100)) {
           		return;
        	}
        	if (obj is Packet) {
            	Packet packet = (Packet) obj;
				if (packet.name == null) {
					
                    Object eventType = Hazelcast.Client.IO.IOUtil.toObject(packet.value);
					if (0.Equals(eventType) || 2.Equals(eventType)) {
                        instanceListenerManager.notifyListeners(packet);
                    } else {
                        membershipListenerManager.notifyListeners(packet);
                    }
				}else if(getInstanceType(packet.name).Equals(InstanceType.QUEUE)){
					queueItemListenerManager.notifyListeners(packet);			
				}else if(getInstanceType(packet.name).Equals(InstanceType.TOPIC)){
					messageListenerManager.notifyListeners(packet);
				}else {
            		entryListenerManager.notifyListeners(packet);
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
		
		public ItemListenerManager getItemListenerManager(){
			return itemListenerManager;
		}
		
		public MembershipListenerManager getMembershipListenerManager(){
			return membershipListenerManager;	
		}
		
		public InstanceListenerManager getInstanceListenerManager() {
	        return instanceListenerManager;
	    }
		
		public ListenerManager start (String prefix)
		{
			Thread thread = new Thread (new ThreadStart (this.run));
			thread.Name = prefix + "Listener";
			thread.Start ();
			return this;
		}
	}
}

