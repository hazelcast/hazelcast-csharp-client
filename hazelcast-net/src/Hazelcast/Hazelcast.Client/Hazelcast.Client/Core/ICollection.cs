using System;
using Hazelcast.Core;

namespace Hazelcast.Core
{
	public interface ICollection<E>
	{
		
		int size();
		
		String getName();
			
		void addItemListener(ItemListener<Object> listener, bool includeValue);
			
		void removeItemListener(ItemListener<Object> listener);
	}
}

