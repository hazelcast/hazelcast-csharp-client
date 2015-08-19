namespace Hazelcast.Client.Proxy
{
    using Core;

    using IO.Serialization;

    using Protocol;
    using Protocol.Codec;

    using Spi;

    internal class ClientCountDownLatchProxy : ClientProxy, ICountDownLatch
    {
        private volatile IData key;

        public ClientCountDownLatchProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool Await(long timeout, TimeUnit unit)
        {            
            var request = CountDownLatchAwaitCodec.EncodeRequest(GetName(), GetTimeInMillis(timeout, unit));
            var response = Invoke(request);
            return CountDownLatchAwaitCodec.DecodeResponse(response).response;            
        }

        public virtual void CountDown()
        {
            var request = CountDownLatchCountDownCodec.EncodeRequest(GetName());            
            Invoke(request);
        }

        public virtual int GetCount()
        {
            var request = CountDownLatchGetCountCodec.EncodeRequest(GetName());
            var response = Invoke(request);
            return CountDownLatchGetCountCodec.DecodeResponse(response).response;            
        }

        public virtual bool TrySetCount(int count)
        {
            var request = CountDownLatchTrySetCountCodec.EncodeRequest(GetName(), count);
            var response = Invoke(request);
            return CountDownLatchTrySetCountCodec.DecodeResponse(response).response;            
        }

        private IData GetKey()
        {
            if (key == null)
            {
                key = ToData(GetName());
            }
            return key;
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return Invoke(request, GetKey());
        }

        private long GetTimeInMillis(long time, TimeUnit timeunit)
        {
            return timeunit != null ? timeunit.ToMillis(time) : time;
        }
    }
}