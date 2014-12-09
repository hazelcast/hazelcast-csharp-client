using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Concurrent.Semaphore;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientSemaphoreProxy : ClientProxy, ISemaphore
    {
        private readonly string name;

        private volatile IData key;

        public ClientSemaphoreProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
            name = objectId;
        }

        public virtual bool Init(int permits)
        {
            CheckNegative(permits);
            var request = new InitRequest(name, permits);
            var result = Invoke<bool>(request);
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
            var request = new AcquireRequest(name, permits, -1);
            Invoke<object>(request);
        }

        public virtual int AvailablePermits()
        {
            var request = new AvailableRequest(name);
            var result = Invoke<int>(request);
            return result;
        }

        public virtual int DrainPermits()
        {
            var request = new DrainRequest(name);
            var result = Invoke<int>(request);
            return result;
        }

        public virtual void ReducePermits(int reduction)
        {
            CheckNegative(reduction);
            var request = new ReduceRequest(name, reduction);
            Invoke<object>(request);
        }

        public virtual void Release()
        {
            Release(1);
        }

        public virtual void Release(int permits)
        {
            CheckNegative(permits);
            var request = new ReleaseRequest(name, permits);
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
            var request = new AcquireRequest(name, permits, unit.ToMillis(timeout));
            var result = Invoke<bool>(request);
            return result;
        }

        protected override void OnDestroy()
        {
        }

        protected override T Invoke<T>(ClientRequest request)
        {
            return base.Invoke<T>(request, GetPartitionKey());
        }

        public virtual IData GetPartitionKey()
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