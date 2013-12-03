using System;

namespace Hazelcast.Core
{
	public class Message<E>
	{
		private readonly E messageObject;

    	public Message(E messageObject) {
        	this.messageObject = messageObject;
    	}
	
	    public virtual E getMessageObject() {
	        return messageObject;
	    }
	}
}

