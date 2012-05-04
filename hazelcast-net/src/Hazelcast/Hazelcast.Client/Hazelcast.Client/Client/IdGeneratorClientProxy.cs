using System;
using System.Threading;
using Hazelcast.Client.IO;
using Hazelcast.Client.Impl;
using Hazelcast.Core;

namespace Hazelcast.Client
{
	public class IdGeneratorClientProxy: IdGenerator
	{
		private String name;
		private ProxyHelper proxyHelper;
		private ListenerManager lManager;
		
		public IdGeneratorClientProxy (OutThread outThread, String name, ListenerManager listenerManager, HazelcastClient client)
		{
			this.name = name;
			this.proxyHelper = new ProxyHelper(name, outThread, lManager, client);
			this.lManager = listenerManager;
		}
		
		public String getName() {
        	return name.Substring(Prefix.IDGEN.Length);
	    }
	
	    public long newId() {
	        return proxyHelper.doOp<long>(ClusterOperation.NEW_ID, null, null);
	    }
	
	    public InstanceType getInstanceType() {
	        return InstanceType.ID_GENERATOR;
	    }
	
	    public void destroy() {
	        proxyHelper.destroy();
	    }
	
	    public Object getId() {
	        return name;
	    }
	}
}

