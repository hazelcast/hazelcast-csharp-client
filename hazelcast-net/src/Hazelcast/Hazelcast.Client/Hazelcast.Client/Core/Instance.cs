using System;
using Hazelcast.Core;

namespace Hazelcast.Core
{
	public interface Instance
	{
	
		InstanceType getInstanceType();
	
	    /**
	     * Destroys this instance cluster-wide.
	     * Clears and releases all resources for this instance.
	     */
	    void destroy();
	
	    /**
	     * Returns the unique id for this instance.
	     *
	     * @return id the of this instance
	     */
	    Object getId();
			
	}
}

