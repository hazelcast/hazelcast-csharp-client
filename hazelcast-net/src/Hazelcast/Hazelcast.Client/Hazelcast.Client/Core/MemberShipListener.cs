using System;

namespace Hazelcast.Core
{
	public interface MembershipListener
	{
		 /**
	     * Invoked when a new member is added to the cluster.
	     *
	     * @param membershipEvent membership event
	     */
	    void memberAdded(MembershipEvent membershipEvent);
	
	    /**
	     * Invoked when an existing member leaves the cluster.
	     *
	     * @param membershipEvent membership event
	     */
	    void memberRemoved(MembershipEvent membershipEvent);
	}
}

