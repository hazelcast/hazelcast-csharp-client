using System;

namespace Hazelcast.Core
{
	public class ItemEvent<E>
	{
		private E item;
		private ItemEventType eventType;
		private String name;
		public ItemEventType EventType {
			get {
				return this.eventType;
			}
		}

		public virtual E Item {
			get {
				return this.item;
			}
			set{this.item = item;}
		}

		public ItemEvent(String name, ItemEventType itemEventType, E item) 
		{
			this.name = name;
        	this.item = item;
        	this.eventType = itemEventType;
    	}
		
		
		
    	
	}
}

