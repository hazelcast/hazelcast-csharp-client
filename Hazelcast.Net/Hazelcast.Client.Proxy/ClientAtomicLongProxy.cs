using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Concurrent.Atomiclong;
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
            var request = new AddAndGetRequest(name, delta);
            var result = Invoke<long>(request);
            return result;
        }

        public virtual bool CompareAndSet(long expect, long update)
        {
            var request = new CompareAndSetRequest(name, expect, update);
            var result = Invoke<bool>(request);
            return result;
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
            var request = new GetAndAddRequest(name, delta);
            var result = Invoke<long>(request);
            return result;
        }

        public virtual long GetAndSet(long newValue)
        {
            var request = new GetAndSetRequest(name, newValue);
            var result = Invoke<long>(request);
            return result;
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
            var request = new SetRequest(name, newValue);
            Invoke<object>(request);
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

        protected override T Invoke<T>(ClientRequest req)
        {
            return Invoke<T>(req, GetKey());
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