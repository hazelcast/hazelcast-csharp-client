using System;
using Hazelcast.Core;
using Hazelcast.Client.Impl;

namespace Hazelcast.Client
{
	public class TopicClientProxy<E>: ITopic<E>
	{
		
		private String name;
		private ProxyHelper proxyHelper;
		private ListenerManager lManager;
		
		public TopicClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client)
		{
			this.name = name;
			this.proxyHelper = new ProxyHelper(name, outThread, lManager, client);
			this.lManager = listenerManager;
		}
		
		public String getName(){
			return this.name.Substring(Prefix.TOPIC.Length);
		}
		
		public InstanceType getInstanceType(){
			return InstanceType.TOPIC;
		}
	
	    public void destroy(){
			proxyHelper.destroy();
		}
	
	    public Object getId(){
			return name;
		}
	

	    public void publish(E message){
			ProxyHelper.check(message);
			proxyHelper.doFireAndForget(ClusterOperation.TOPIC_PUBLISH, message, null);
		}
	
	    public void addMessageListener(MessageListener<Object> listener){
			lock (name) {
	            bool shouldCall = messageListenerManager().noListenerRegistered(name);
	            messageListenerManager().registerListener(name, listener);
	            if (shouldCall) {
	                doAddListenerCall(listener);
	            }
	        }
			
		}
		
		private void doAddListenerCall(MessageListener<Object> messageListener) {
	        Call c = messageListenerManager().createNewAddListenerCall(proxyHelper);
	        proxyHelper.doCall(c);
	    }
	
	    public void removeMessageListener(MessageListener<Object> listener){
			lock(name) {
            messageListenerManager().removeListener(name, listener);
            if (messageListenerManager().noListenerRegistered(name)) {
                proxyHelper.doOp<object>(ClusterOperation.REMOVE_LISTENER, null, null);
            }
        }
			
		}
		
		private MessageListenerManager messageListenerManager(){
			return lManager.getMessageListenerManager();
		}
	}
}

