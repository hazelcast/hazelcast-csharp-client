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
    internal class ClientAtomicLongProxy : ClientProxy, IAtomicLong
    {
        private readonly string _name;
        private volatile IData _key;

        public ClientAtomicLongProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
            _name = objectId;
        }

        public virtual long AddAndGet(long delta)
        {
            var request = AtomicLongAddAndGetCodec.EncodeRequest(_name, delta);
            return Invoke(request, m => AtomicLongAddAndGetCodec.DecodeResponse(m).Response);
        }

        public virtual bool CompareAndSet(long expect, long update)
        {
            var request = AtomicLongCompareAndSetCodec.EncodeRequest(_name, expect, update);
            return Invoke(request, m => AtomicLongCompareAndSetCodec.DecodeResponse(m).Response);
        }

        public virtual long DecrementAndGet()
        {
            var request = AtomicLongDecrementAndGetCodec.EncodeRequest(_name);
            return Invoke(request, m => AtomicLongDecrementAndGetCodec.DecodeResponse(m).Response);
        }

        public virtual long Get()
        {
            var request = AtomicLongGetCodec.EncodeRequest(_name);
            return Invoke(request, m => AtomicLongGetCodec.DecodeResponse(m).Response);
        }

        public virtual long GetAndAdd(long delta)
        {
            var request = AtomicLongGetAndAddCodec.EncodeRequest(_name, delta);
            return Invoke(request, m => AtomicLongGetAndAddCodec.DecodeResponse(m).Response);
        }

        public virtual long GetAndSet(long newValue)
        {
            var request = AtomicLongGetAndSetCodec.EncodeRequest(_name, newValue);
            return Invoke(request, m => AtomicLongGetAndSetCodec.DecodeResponse(m).Response);
        }

        public virtual long IncrementAndGet()
        {
            var request = AtomicLongIncrementAndGetCodec.EncodeRequest(_name);
            return Invoke(request, m => AtomicLongIncrementAndGetCodec.DecodeResponse(m).Response);
        }

        public virtual long GetAndIncrement()
        {
            var request = AtomicLongGetAndIncrementCodec.EncodeRequest(_name);
            return Invoke(request, m => AtomicLongGetAndIncrementCodec.DecodeResponse(m).Response);
        }

        public virtual void Set(long newValue)
        {
            var request = AtomicLongSetCodec.EncodeRequest(_name, newValue);
            Invoke(request);
        }

        protected override ClientMessage Invoke(ClientMessage request)
        {
            return Invoke(request, GetKey());
        }

        private IData GetKey()
        {
            if (_key == null)
            {
                _key = ToData(_name);
            }
            return _key;
        }
    }
}