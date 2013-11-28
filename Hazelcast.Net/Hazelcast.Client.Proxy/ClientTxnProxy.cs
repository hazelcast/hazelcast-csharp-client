using System.IO;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Partition.Strategy;
using Hazelcast.Transaction;
using Hazelcast.Util;


namespace Hazelcast.Client.Proxy
{
    public abstract class ClientTxnProxy : ITransactionalObject
	{
		internal readonly string objectName;

		internal readonly TransactionContextProxy proxy;

		internal ClientTxnProxy(string objectName, TransactionContextProxy proxy)
		{
			this.objectName = objectName;
			this.proxy = proxy;
		}

		internal T Invoke<T>(object request)
		{
			ClientClusterService clusterService = (ClientClusterService)proxy.GetClient().GetClientClusterService();
			try
			{
				return clusterService.SendAndReceiveFixedConnection<T>(proxy.GetConnection(), request);
			}
			catch (IOException e)
			{
				throw ExceptionUtil.Rethrow(new HazelcastException(e));
			}
		}

		internal abstract void OnDestroy();

		public void Destroy()
		{
			OnDestroy();
			ClientDestroyRequest request = new ClientDestroyRequest(objectName, GetServiceName());
			Invoke<object>(request);
		}

		public virtual object GetId()
		{
			return objectName;
		}

		public virtual string GetPartitionKey()
		{
			return StringPartitioningStrategy.GetPartitionKey(GetName());
		}

		internal virtual Data ToData(object obj)
		{
			return proxy.GetClient().GetSerializationService().ToData(obj);
		}

		internal virtual object ToObject(Data data)
		{
			return proxy.GetClient().GetSerializationService().ToObject(data);
		}

		public abstract string GetName();

		public abstract string GetServiceName();
	}
}
