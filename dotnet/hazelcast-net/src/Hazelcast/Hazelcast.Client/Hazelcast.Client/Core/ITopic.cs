using System;

namespace Hazelcast.Client
{
	public interface ITopic<E>
	{
		String getName();

	    void publish(E message);
	
	    void addMessageListener(MessageListener<E> listener);
	
	    void removeMessageListener(MessageListener<E> listener);
	}
}

