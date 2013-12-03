using System;
using Hazelcast.IO;
using Hazelcast.Client.IO;

namespace Hazelcast.Core
{
	public class DataAwareItemEvent<E>: ItemEvent<E>
	{
		readonly Data itemData;
		public DataAwareItemEvent(String name, ItemEventType itemEventType, Data itemData):base(name, itemEventType, default(E))
		{
			this.itemData = itemData;
		}
		
		public override E Item {
			get {
				return (E)IOUtil.toObject(itemData.Buffer);
			}
			set{this.Item = Item;}
		}
		
		
	}
}

