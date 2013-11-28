using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Partition.Strategy;
using Hazelcast.Util;
using System.Collections.Concurrent;


namespace Hazelcast.Client.Spi
{
	public abstract class ClientProxy : IDistributedObject
	{
		private readonly string serviceName;

		private readonly string objectName;

		private volatile ClientContext context;

        private readonly ConcurrentDictionary<string, IListenerSupport> listenerSupportMap = new ConcurrentDictionary<string, IListenerSupport>();

		protected internal ClientProxy(string serviceName, string objectName)
		{
			this.serviceName = serviceName;
			this.objectName = objectName;
		}

		protected internal string Listen<T>(object registrationRequest, object partitionKey, EventHandler<T> handler)
		{
			var listenerSupport = new ListenerSupport<T>(context, registrationRequest, handler, partitionKey);
			string registrationId = listenerSupport.Listen();
			listenerSupportMap.TryAdd(registrationId, listenerSupport);
			return registrationId;
		}

		protected internal string Listen<T>(object registrationRequest, EventHandler<T> handler)
		{
			return Listen(registrationRequest, null, handler);
		}

		protected internal bool StopListening(string registrationId)
		{
            IListenerSupport listenerSupport = null;
            listenerSupportMap.TryRemove(registrationId, out listenerSupport);
			if (listenerSupport != null)
			{
				listenerSupport.Stop();
				return true;
			}
			return false;
		}

		protected internal ClientContext GetContext()
		{
			ClientContext ctx = context;
			if (ctx == null)
			{
				throw new HazelcastInstanceNotActiveException();
			}
			return context;
		}

		internal void SetContext(ClientContext context)
		{
			this.context = context;
		}

		[Obsolete]
		public object GetId()
		{
			return objectName;
		}

		public string GetName()
		{
			return objectName;
		}

		public string GetPartitionKey()
		{
			return StringPartitioningStrategy.GetPartitionKey(GetName());
		}

		//REQUIRED
		public string GetServiceName()
		{
			return serviceName;
		}

		public void Destroy()
		{
			OnDestroy();
			ClientDestroyRequest request = new ClientDestroyRequest(objectName, GetServiceName());
			try
			{
				context.GetInvocationService().InvokeOnRandomTarget<Object>(request);
			}
			catch (Exception e)
			{
				throw ExceptionUtil.Rethrow(e);
			}
			context.RemoveProxy(this);
			context = null;
		}

		protected internal abstract void OnDestroy();
	}
}
