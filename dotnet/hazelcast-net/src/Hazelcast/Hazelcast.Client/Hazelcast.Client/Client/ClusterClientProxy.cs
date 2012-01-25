using System;
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Client.Impl;
using Hazelcast.Impl;
using Hazelcast.Impl.Base;
using Hazelcast.IO;
using Hazelcast.Client.IO;

namespace Hazelcast.Client
{
	public class ClusterClientProxy: ICluster {
	    ProxyHelper proxyHelper;
	    private HazelcastClient client;
		private ListenerManager listenerManager;
	
	    public ClusterClientProxy(OutThread outThread, ListenerManager lManager, HazelcastClient client) {
	        this.client = client;
	        proxyHelper = new ProxyHelper("", outThread,lManager, client);
	    	this.listenerManager = lManager;
		}
	
	    public System.Collections.Generic.ICollection<Instance> getInstances() {
	        Keys instances = proxyHelper.doOp<Keys>(ClusterOperation.GET_INSTANCES, null, null);
	        List<Instance> list = new List<Instance>();
	        if (instances != null) {
	            for (int i = 0; i < instances.Count(); i++) {
	                Object inst = IOUtil.toObject(instances.Get(i).Buffer);
					if (inst is ProxyKey) {
	                    ProxyKey proxyKey = (ProxyKey) inst;
						Object instance = client.getClientProxy(proxyKey.Key);
						if(instance!=null)
	                    	list.Add((Instance) instance);
	                } else {	                    
	                	Object instance = client.getClientProxy(inst);
						if(instance!=null)
	                    	list.Add((Instance) instance);
					}
	            }
	        }
	        return list;
	    }
	
	    public void addMembershipListener(MembershipListener listener) {
	        listenerManager.getMembershipListenerManager().registerListener(listener);
	    }
	
	    public void removeMembershipListener(MembershipListener listener) {
	        listenerManager.getMembershipListenerManager().removeListener(listener);
	    }
	
	    public System.Collections.Generic.ICollection<Member> getMembers() {
	        Keys cw = proxyHelper.doOp<Keys>(ClusterOperation.GET_MEMBERS, null, null);
	        List<Member> set = new List<Member>();
	        for(int i=0;i< cw.Count() ;i++){
				Data d = cw.Get(i);
	            set.Add((Member)IOUtil.toObject(d.Buffer));
	        }
	        return set;
	    }
	
	    public long getClusterTime() {
	        return proxyHelper.doOp<long>(ClusterOperation.GET_CLUSTER_TIME, null, null);
	    }
	
	    public void addInstanceListener(InstanceListener listener) {
			Console.WriteLine("Listener Registered! " + instanceListenerManager().noListenerRegistered());
	        if (instanceListenerManager().noListenerRegistered()) {
	            Call c = instanceListenerManager().createNewAddListenerCall(proxyHelper);
	            proxyHelper.doCall(c);
	        }
	        instanceListenerManager().registerListener(listener);
	    }
	
	    public void removeInstanceListener(InstanceListener instanceListener) {
	        instanceListenerManager().removeListener(instanceListener);
	    }
	
	    private InstanceListenerManager instanceListenerManager() {
	        return listenerManager.getInstanceListenerManager();
	    }
	
	    public override String ToString() {
	        System.Collections.Generic.ICollection<Member> members = getMembers();
	        System.Text.StringBuilder sb = new System.Text.StringBuilder("Cluster [");
	        if (members != null) {
	            sb.Append(members.Count);
	            sb.Append("] {");
	            foreach (Member member in members) {
	                sb.Append("\n\t").Append(member);
	            }
	        }
	        sb.Append("\n}\n");
	        return sb.ToString();
	    }
	}
}

