using System;
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Client.IO;

namespace Hazelcast.Client
{
	public class MembershipListenerManager
	{
		private List<MembershipListener> memberShipListeners = new List<MembershipListener>();
	    private HazelcastClient client;
		private Object lockObject = new Object();
	
	    public MembershipListenerManager(HazelcastClient client) {
	        this.client = client;
		}
	
	    public void registerListener(MembershipListener listener) {
	        lock(lockObject){
				this.memberShipListeners.Add(listener);
			}
		}
	
	    public void removeListener(MembershipListener listener) {
	        lock(lockObject){
				this.memberShipListeners.Remove(listener);
			}
	    }
	
	    public bool noListenerRegistered() {
			lock(lockObject){
	        	return memberShipListeners.Count==0;
			}
		}
	
	    public void notifyListeners(Packet packet) {
	        if (memberShipListeners.Count > 0) {
	            Member member = (Member) IOUtil.toObject(packet.key);
	            int type = (int) IOUtil.toObject(packet.value);
	            MembershipEvent e= new MembershipEvent(client.getCluster(), member, type);
	            if (type.Equals(MembershipEvent.MEMBER_ADDED))
	                foreach (MembershipListener membershipListener in memberShipListeners) 
	                    membershipListener.memberAdded(e);
	                
	            else 
	                foreach (MembershipListener membershipListener in memberShipListeners) 
	                    membershipListener.memberRemoved(e);
	                
	        }
	    }
	}
}

