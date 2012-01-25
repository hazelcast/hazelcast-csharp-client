using System;

namespace Hazelcast.Core
{
	public interface MessageListener<out E>
	{
		 void onMessage<E>(Message<E> message);
	}
}

