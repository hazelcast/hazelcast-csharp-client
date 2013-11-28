using System;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Request.Queue;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;


namespace Hazelcast.Client.Proxy
{
	public class ClientTxnQueueProxy<E> : ClientTxnProxy, ITransactionalQueue<E>
	{
		public ClientTxnQueueProxy(string name, TransactionContextProxy proxy) : base(name, proxy)
		{
		}

		public virtual bool Offer(E e)
		{
			try
			{
				return Offer(e, 0, TimeUnit.MILLISECONDS);
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <exception cref="System.Exception"></exception>
		public virtual bool Offer(E e, long timeout, TimeUnit unit)
		{
			Data data = ToData(e);
			TxnOfferRequest request = new TxnOfferRequest(GetName(), unit.ToMillis(timeout), data);
			bool result = Invoke<bool>(request);
			return result;
		}

		public virtual E Poll()
		{
			try
			{
                return Poll(0, TimeUnit.MILLISECONDS);
			}
			catch (Exception)
			{
				return default(E);
			}
		}

		/// <exception cref="System.Exception"></exception>
		public virtual E Poll(long timeout, TimeUnit unit)
		{
			TxnPollRequest request = new TxnPollRequest(GetName(), unit.ToMillis(timeout));
			return Invoke<E>(request);
		}

		public virtual E Peek()
		{
			try
			{
                return Peek(0, TimeUnit.MILLISECONDS);
			}
			catch (Exception)
			{
				return default(E);
			}
		}

		/// <exception cref="System.Exception"></exception>
		public virtual E Peek(long timeout, TimeUnit unit)
		{
			TxnPeekRequest request = new TxnPeekRequest(GetName(), unit.ToMillis(timeout));
			return Invoke<E>(request);
		}

		public virtual int Size()
		{
			TxnSizeRequest request = new TxnSizeRequest(GetName());
			int result = Invoke<int>(request);
			return result;
		}

		public override string GetName()
		{
			return (string)GetId();
		}

		public override string GetServiceName()
		{
			return ServiceNames.Queue;
		}

		internal override void OnDestroy()
		{
		}
	}
}
