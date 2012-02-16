using System;
namespace Hazelcast.Core
{
	public interface ITopic<E>: Instance
	{
		String getName();

	    void publish(E message);
	
	    void addMessageListener(MessageListener<Object> listener);
	
	    void removeMessageListener(MessageListener<Object> listener);
	}
}

