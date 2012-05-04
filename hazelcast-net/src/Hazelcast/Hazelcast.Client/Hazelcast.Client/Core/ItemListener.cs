using System;

namespace Hazelcast.Core
{
	public interface ItemListener<out E>
	{
		void itemAdded<E>(ItemEvent<E> item);
		void itemRemoved<E>(ItemEvent<E> item);
	}
	
	/*public interface ItemListener{
		void itemAdded(ItemEvent<Object> item);
		void itemRemoved(ItemEvent<Object> item);
	}*/
}

