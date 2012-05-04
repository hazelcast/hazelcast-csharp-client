using System;

namespace Hazelcast.Core
{
	public interface InstanceListener
	{
		void instanceCreated(InstanceEvent e);

    	void instanceDestroyed(InstanceEvent e);
	}
}

