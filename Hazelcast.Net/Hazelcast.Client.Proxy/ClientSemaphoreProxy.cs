using System;
using Hazelcast.Client.Request.Concurrent.Semaphore;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Util;


namespace Hazelcast.Client.Proxy
{
	public class ClientSemaphoreProxy : ClientProxy, ISemaphore
	{
		private readonly string name;

		private volatile Data key;

		public ClientSemaphoreProxy(string serviceName, string objectId) : base(serviceName, objectId)
		{
			this.name = objectId;
		}

		public virtual bool Init(int permits)
		{
			CheckNegative(permits);
			InitRequest request = new InitRequest(name, permits);
			bool result = Invoke<bool>(request);
			return result;
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void Acquire()
		{
			Acquire(1);
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void Acquire(int permits)
		{
			CheckNegative(permits);
			AcquireRequest request = new AcquireRequest(name, permits, -1);
			Invoke<object>(request);
		}

		public virtual int AvailablePermits()
		{
			AvailableRequest request = new AvailableRequest(name);
			int result = Invoke<int>(request);
			return result;
		}

		public virtual int DrainPermits()
		{
			DrainRequest request = new DrainRequest(name);
			int result = Invoke<int>(request);
			return result;
		}

		public virtual void ReducePermits(int reduction)
		{
			CheckNegative(reduction);
			ReduceRequest request = new ReduceRequest(name, reduction);
			Invoke<object>(request);
		}

		public virtual void Release()
		{
			Release(1);
		}

		public virtual void Release(int permits)
		{
			CheckNegative(permits);
			ReleaseRequest request = new ReleaseRequest(name, permits);
			Invoke<object>(request);
		}

		public virtual bool TryAcquire()
		{
			return TryAcquire(1);
		}

		public virtual bool TryAcquire(int permits)
		{
			CheckNegative(permits);
			try
			{
				return TryAcquire(permits, 0, TimeUnit.SECONDS);
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <exception cref="System.Exception"></exception>
		public virtual bool TryAcquire(long timeout, TimeUnit unit)
		{
			return TryAcquire(1, timeout, unit);
		}

		/// <exception cref="System.Exception"></exception>
		public virtual bool TryAcquire(int permits, long timeout, TimeUnit unit)
		{
			CheckNegative(permits);
			AcquireRequest request = new AcquireRequest(name, permits, unit.ToMillis(timeout));
			bool result = Invoke<bool>(request);
			return result;
		}

		protected internal override void OnDestroy()
		{
		}

		private T Invoke<T>(object req)
		{
			try
			{
				return GetContext().GetInvocationService().InvokeOnKeyOwner<T>(req, GetKey());
			}
			catch (Exception e)
			{
				throw ExceptionUtil.Rethrow(e);
			}
		}

		public virtual Data GetKey()
		{
			if (key == null)
			{
				key = GetContext().GetSerializationService().ToData(name);
			}
			return key;
		}

		private void CheckNegative(int permits)
		{
			if (permits < 0)
			{
				throw new ArgumentException("Permits cannot be negative!");
			}
		}
	}
}
