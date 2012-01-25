using System;
using Hazelcast.Core;

namespace Hazelcast.Core
{
	public interface ICollection<E>
	{
		
		int size();
		
		String getName();
			
		void addItemListener(ItemListener<E> listener, bool includeValue);
			
		void removeItemListener(ItemListener<E> listener);
	}
}

