using System;

namespace Hazelcast.Core
{
	public interface ILock: Instance
	{
		void Lock();
		bool tryLock();
		bool tryLock(long time);
		void unLock();
		Object getLockObject();
	}
}

