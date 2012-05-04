using System;

namespace Hazelcast.Core
{
	public class MembershipEvent
	{
		public static int MEMBER_ADDED = 1;

	    public static int MEMBER_REMOVED = 3;
	
	    private Member member;
	
	    private int eventType;
		
		private ICluster cluster;
	
	    public MembershipEvent(ICluster cluster, Member member, int eventType) {
	        this.cluster = cluster;
	        this.member = member;
	        this.eventType = eventType;
	    }
	
	    /**
	     * Returns the cluster of the event.
	     *
	     * @return
	     */
	    public ICluster getCluster() {
	        return cluster;
	    }
	
	    /**
	     * Returns the membership event type; #MEMBER_ADDED or #MEMBER_REMOVED
	     *
	     * @return the membeship event type
	     */
	    public int getEventType() {
	        return eventType;
	    }
	
	    /**
	     * Returns the removed or added member.
	     *
	     * @return member which is removed/added
	     */
	    public Member getMember() {
	        return member;
	    }
	
	    public override String ToString() {
	        return "MembershipEvent {" + member + "} "
	                + ((eventType == MEMBER_ADDED) ? "added" : "removed");
	    }
	}
}

