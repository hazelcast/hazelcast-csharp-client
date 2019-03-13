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

using System;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    internal class ClientSemaphoreProxy : ClientProxy, ISemaphore
    {
        private volatile IData _key;

        public ClientSemaphoreProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
        }

        public virtual bool Init(int permits)
        {
            CheckNegative(permits);
            var request = SemaphoreInitCodec.EncodeRequest(GetName(), permits);
            return Invoke(request, m => SemaphoreInitCodec.DecodeResponse(m).response);
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
            var request = SemaphoreAcquireCodec.EncodeRequest(GetName(), permits);
            Invoke(request);
        }

        public virtual int AvailablePermits()
        {
            var request = SemaphoreAvailablePermitsCodec.EncodeRequest(GetName());
            return Invoke(request, m => SemaphoreAvailablePermitsCodec.DecodeResponse(m).response);
        }

        public virtual int DrainPermits()
        {
            var request = SemaphoreDrainPermitsCodec.EncodeRequest(GetName());
            return Invoke(request, m => SemaphoreDrainPermitsCodec.DecodeResponse(m).response);
        }

        public virtual void ReducePermits(int reduction)
        {
            CheckNegative(reduction);
            var request = SemaphoreReducePermitsCodec.EncodeRequest(GetName(), reduction);
            Invoke(request);
        }

        public virtual void Release()
        {
            Release(1);
        }

        public virtual void Release(int permits)
        {
            CheckNegative(permits);
            var request = SemaphoreReleaseCodec.EncodeRequest(GetName(), permits);
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
                return TryAcquire(permits, 0, TimeUnit.Seconds);
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
            var request = SemaphoreTryAcquireCodec.EncodeRequest(GetName(), permits, unit.ToMillis(timeout));
            return Invoke(request, m => SemaphoreTryAcquireCodec.DecodeResponse(m).response);
        }

        public IData GetKey()
        {
            if (_key == null)
            {
                _key = GetContext().GetSerializationService().ToData(GetName());
            }
            return _key;
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetKey());
        }

        // ReSharper disable once UnusedParameter.Local
        private void CheckNegative(int permits)
        {
            if (permits < 0)
            {
                throw new ArgumentException("Permits cannot be negative!");
            }
        }
    }
}