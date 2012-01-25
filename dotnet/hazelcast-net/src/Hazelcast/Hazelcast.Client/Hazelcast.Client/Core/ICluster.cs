using System;

namespace Hazelcast.Core
{
	public interface ICluster
	{
		
	    /**
	     * Adds MembershipListener to listen for membership updates.
	     *
	     * @param listener membership listener
	     */
	    void addMembershipListener(MembershipListener listener);
	
	    /**
	     * Removes the specified membership listener.
	     *
	     * @param listener membership listener to remove
	     */
	    void removeMembershipListener(MembershipListener listener);
	
	    /**
	     * Set of current members of the cluster.
	     * Returning set instance is not modifiable.
	     * Every member in the cluster has the same member list in the same
	     * order. First member is the oldest member.
	     *
	     * @return current members of the cluster
	     */
	    System.Collections.Generic.ICollection<Member> getMembers();
		
	
	    /**
	     * Returns the cluster-wide time.
	     * <p/>
	     * Cluster tries to keep a cluster-wide time which is
	     * might be different than the member's own system time.
	     * Cluster-wide time is -almost- the same on all members
	     * of the cluster.
	     *
	     * @return
	     */
	    long getClusterTime();
	}
}

