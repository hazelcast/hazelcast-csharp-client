using System;

namespace Hazelcast.Client
{
	public interface MessageListener<out E>
	{
		 void onMessage<E>(Message<E> message);
	}
}

