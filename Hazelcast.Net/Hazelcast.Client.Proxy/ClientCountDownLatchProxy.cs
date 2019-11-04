// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    internal class ClientCountDownLatchProxy : ClientProxy, ICountDownLatch
    {
        private volatile IData _key;

        public ClientCountDownLatchProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool Await(long timeout, TimeUnit unit)
        {
            var request = CountDownLatchAwaitCodec.EncodeRequest(GetName(), GetTimeInMillis(timeout, unit));
            var response = Invoke(request);
            return CountDownLatchAwaitCodec.DecodeResponse(response).Response;
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
            return CountDownLatchGetCountCodec.DecodeResponse(response).Response;
        }

        public virtual bool TrySetCount(int count)
        {
            var request = CountDownLatchTrySetCountCodec.EncodeRequest(GetName(), count);
            var response = Invoke(request);
            return CountDownLatchTrySetCountCodec.DecodeResponse(response).Response;
        }

        protected override ClientMessage Invoke(ClientMessage request)
        {
            return Invoke(request, GetKey());
        }

        private IData GetKey()
        {
            if (_key == null)
            {
                _key = ToData(GetName());
            }
            return _key;
        }

        private long GetTimeInMillis(long time, TimeUnit timeunit)
        {
            return timeunit.ToMillis(time);
        }
    }
}