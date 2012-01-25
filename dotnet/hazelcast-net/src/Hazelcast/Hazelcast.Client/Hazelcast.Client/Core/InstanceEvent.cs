using System;

namespace Hazelcast.Core
{
	public class InstanceEvent
	{
	    private InstanceEventType instanceEventType;
	    private Instance instance;
	
	    public InstanceEvent(InstanceEventType instanceEventType, Instance instance) {
	        this.instanceEventType = instanceEventType;
	        this.instance = instance;
	    }
	
	    public InstanceEventType getEventType() {
	        return instanceEventType;
	    }
	
	    public InstanceType getInstanceType() {
	        return instance.getInstanceType();
	    }
	
	    public Instance getInstance() {
	        return instance;
	    }
		
		public override string ToString ()
		{
			return string.Format ("InstanceEvent +["+ instanceEventType+ ": "+instance.getId()+"]");
		}
	}
}

