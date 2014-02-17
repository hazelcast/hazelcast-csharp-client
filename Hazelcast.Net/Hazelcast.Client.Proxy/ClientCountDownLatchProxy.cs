using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Concurrent.Countdownlatch;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientCountDownLatchProxy : ClientProxy, ICountDownLatch
    {
        private volatile Data key;

        public ClientCountDownLatchProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool Await(long timeout, TimeUnit unit)
        {
            var request = new AwaitRequest(GetName(), GetTimeInMillis(timeout, unit));
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual void CountDown()
        {
            var request = new CountDownRequest(GetName());
            Invoke<object>(request);
        }

        public virtual int GetCount()
        {
            var request = new GetCountRequest(GetName());
            var result = Invoke<int>(request);
            return result;
        }

        public virtual bool TrySetCount(int count)
        {
            var request = new SetCountRequest(GetName(), count);
            var result = Invoke<bool>(request);
            return result;
        }

        protected override void OnDestroy()
        {
        }

        private Data GetKey()
        {
            if (key == null)
            {
                key = ToData(GetName());
            }
            return key;
        }

        private long GetTimeInMillis(long time, TimeUnit timeunit)
        {
            return timeunit != null ? timeunit.ToMillis(time) : time;
        }

        protected override T Invoke<T>(ClientRequest req)
        {
            return Invoke<T>(req, GetKey());
        }
    }
}