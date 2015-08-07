using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    internal class ClientAtomicLongProxy : ClientProxy, IAtomicLong
    {
        private readonly string name;
        private volatile IData key;

        public ClientAtomicLongProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
            name = objectId;
        }

        public virtual long AddAndGet(long delta)
        {
            var request = AtomicLongAddAndGetCodec.EncodeRequest(name, delta);
            var response = Invoke(request);
            return AtomicLongAddAndGetCodec.DecodeResponse(response).response;
        }

        public virtual bool CompareAndSet(long expect, long update)
        {
            var request = AtomicLongCompareAndSetCodec.EncodeRequest(name, expect, update);
            var response = Invoke(request);
            return AtomicLongCompareAndSetCodec.DecodeResponse(response).response;
        }

        public virtual long DecrementAndGet()
        {
            return AddAndGet(-1);
        }

        public virtual long Get()
        {
            return GetAndAdd(0);
        }

        public virtual long GetAndAdd(long delta)
        {
            var request = AtomicLongGetAndAddCodec.EncodeRequest(name, delta);
            var response = Invoke(request);
            return AtomicLongGetAndAddCodec.DecodeResponse(response).response;
        }

        public virtual long GetAndSet(long newValue)
        {
            var request = AtomicLongGetAndSetCodec.EncodeRequest(name, newValue);
            var response = Invoke(request);
            return AtomicLongGetAndAddCodec.DecodeResponse(response).response;
        }

        public virtual long IncrementAndGet()
        {
            return AddAndGet(1);
        }

        public virtual long GetAndIncrement()
        {
            return GetAndAdd(1);
        }

        public virtual void Set(long newValue)
        {
            var request = AtomicLongSetCodec.EncodeRequest(name, newValue);
            Invoke(request);
        }

        //public void Alter(Func<long, long> function)
        //{
        //    throw new NotImplementedException();
        //}

        //public long AlterAndGet(Func<long, long> function)
        //{
        //    throw new NotImplementedException();
        //}

        //public long GetAndAlter(Func<long, long> function)
        //{
        //    throw new NotImplementedException();
        //}

        //public R Apply<R>(Func<long, R> function)
        //{
        //    throw new NotImplementedException();
        //}

        protected override void OnDestroy()
        {
        }

        private IData GetKey()
        {
            if (key == null)
            {
                key = ToData(name);
            }
            return key;
        }
    }
}