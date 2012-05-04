using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Client.IO;

namespace Hazelcast.Client
{
	public class InstanceListenerManager
	{
		private List<InstanceListener> instanceListeners = new List<InstanceListener>();
    	private HazelcastClient client;
		private Object lockObject = new Object();
	
	    public InstanceListenerManager(HazelcastClient client) {
	        this.client = client;
	    }
	
	    public void registerListener(InstanceListener listener) {
			lock(lockObject){
	        	this.instanceListeners.Add(listener);
			}
		}
	
	    public void removeListener(InstanceListener instanceListener) {
			lock(lockObject){
	        	this.instanceListeners.Remove(instanceListener);
			}
		}
	
	    public bool noListenerRegistered() {
			lock(lockObject){
	        	return instanceListeners.Count == 0;
			}
		}
	
	    public void notifyListeners(Packet packet) {
	        String id = (String) IOUtil.toObject(packet.key);
	        int eventType =  (int)IOUtil.toObject(packet.value);
			InstanceEventType instanceEventType = (InstanceEventType)eventType;
	        InstanceEvent e = new InstanceEvent(instanceEventType, (Instance) client.getClientProxy(id));
	        foreach (InstanceListener listener in instanceListeners) {
	            if(instanceEventType.Equals(InstanceEventType.CREATED))
	                listener.instanceCreated(e);
	          	else
					listener.instanceDestroyed(e);
	        }
	    }
	
	    public Call createNewAddListenerCall(ProxyHelper proxyHelper) {
	        Packet request = proxyHelper.createRequestPacket(ClusterOperation.CLIENT_ADD_INSTANCE_LISTENER, null, null, 0);
	        return proxyHelper.createCall(request);
	    }
	
	    public System.Collections.Generic.ICollection<Call> calls(HazelcastClient client) {
	        List<Call> list  = new List<Call>();
			if (noListenerRegistered())
	            return list;
			
			list.Add(createNewAddListenerCall(new ProxyHelper("", client.OutThread, client.ListenerManager, client)));
	        return list;
	    }
	}
}

