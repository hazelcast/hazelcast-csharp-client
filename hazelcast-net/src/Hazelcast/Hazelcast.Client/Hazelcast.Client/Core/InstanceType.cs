using System;

namespace Hazelcast.Core
{
	public enum InstanceType
	{
		QUEUE = 1, 
		MAP = 2, 
		SET = 3, 
		LIST = 4, 
		LOCK = 5, 
		TOPIC = 6, 
		MULTIMAP =7, 
        ID_GENERATOR = 8, 
		ATOMIC_NUMBER = 9, 
		SEMAPHORE = 10, 
		COUNT_DOWN_LATCH = 11
	}
}

