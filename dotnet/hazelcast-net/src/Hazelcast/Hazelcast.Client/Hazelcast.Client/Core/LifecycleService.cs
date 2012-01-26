using System;

namespace Hazelcast.Core
{
	public interface LifecycleService
	{
		void kill();

    	void shutdown();

    	bool pause();

    	bool resume();

    	void restart();

    	//void addLifecycleListener(LifecycleListener lifecycleListener);

   	 	//void removeLifecycleListener(LifecycleListener lifecycleListener);

    	bool isRunning();
	}
}

