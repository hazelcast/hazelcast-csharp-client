using System;
using Hazelcast.Client.Request.Concurrent.Semaphore;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    using Protocol;
    using Protocol.Codec;

    internal class ClientSemaphoreProxy : ClientProxy, ISemaphore
    {
        private readonly string name;

        private volatile IData _key;

        public ClientSemaphoreProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
            name = objectId;
        }

        public virtual bool Init(int permits)
        {
            CheckNegative(permits);
            var request = SemaphoreInitCodec.EncodeRequest(name, permits);
            return Invoke(request, m=> SemaphoreInitCodec.DecodeResponse(m).response);
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
            var request = SemaphoreAcquireCodec.EncodeRequest(name, permits);
            Invoke(request);
        }

        public virtual int AvailablePermits()
        {
            var request = SemaphoreAvailablePermitsCodec.EncodeRequest(name);
            return Invoke(request, m => SemaphoreAvailablePermitsCodec.DecodeResponse(m).response);
        }

        public virtual int DrainPermits()
        {
            var request = SemaphoreDrainPermitsCodec.EncodeRequest(name);
            return Invoke(request, m => SemaphoreDrainPermitsCodec.DecodeResponse(m).response);
        }

        public virtual void ReducePermits(int reduction)
        {
            CheckNegative(reduction);
            var request = SemaphoreReducePermitsCodec.EncodeRequest(name, reduction);
            Invoke(request);
        }

        public virtual void Release()
        {
            Release(1);
        }

        public virtual void Release(int permits)
        {
            CheckNegative(permits);
            var request = SemaphoreReleaseCodec.EncodeRequest(name, permits);
            Invoke(request);
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
            var request = SemaphoreTryAcquireCodec.EncodeRequest(name, permits, unit.ToMillis(timeout));
            return Invoke(request, m => SemaphoreTryAcquireCodec.DecodeResponse(m).response);
        }
      
        public IData GetKey()
        {
            if (_key == null)
            {
                _key = GetContext().GetSerializationService().ToData(name);
            }
            return _key;
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetKey());
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