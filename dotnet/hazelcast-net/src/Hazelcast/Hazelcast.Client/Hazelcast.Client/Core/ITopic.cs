using System;
namespace Hazelcast.Core
{
	public interface ITopic<E>: Instance
	{
		String getName();

	    void publish(E message);
	
	    void addMessageListener(MessageListener<E> listener);
	
	    void removeMessageListener(MessageListener<E> listener);
	}
}

