// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientLockProxy : ClientProxy, ILock
    {
        private ClientLockReferenceIdGenerator _lockReferenceIdGenerator;

        private volatile IData _key;

        public ClientLockProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
        }

        public virtual bool IsLocked()
        {
            var request = LockIsLockedCodec.EncodeRequest(GetName());
            return Invoke(request, m => LockIsLockedCodec.DecodeResponse(m).response);
        }

        public virtual bool IsLockedByCurrentThread()
        {
            var request = LockIsLockedByCurrentThreadCodec.EncodeRequest(GetName(), ThreadUtil.GetThreadId());
            return Invoke(request, m => LockIsLockedByCurrentThreadCodec.DecodeResponse(m).response);
        }

        public virtual int GetLockCount()
        {
            var request = LockGetLockCountCodec.EncodeRequest(GetName());
            return Invoke(request, m => LockGetLockCountCodec.DecodeResponse(m).response);
        }

        public virtual long GetRemainingLeaseTime()
        {
            var request = LockGetRemainingLeaseTimeCodec.EncodeRequest(GetName());
            return Invoke(request, m => LockGetRemainingLeaseTimeCodec.DecodeResponse(m).response);
        }

        public virtual void Lock(long leaseTime, TimeUnit? timeUnit)
        {
            var request = LockLockCodec.EncodeRequest(GetName(), GetTimeInMillis(leaseTime, timeUnit),
                ThreadUtil.GetThreadId(), _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request);
        }

        public virtual void ForceUnlock()
        {
            var request = LockForceUnlockCodec.EncodeRequest(GetName(), _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request);
        }

        public virtual void Lock()
        {
            Lock(-1, null);
        }

        public virtual bool TryLock()
        {
            try
            {
                return TryLock(0, null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool TryLock(long time, TimeUnit? unit)
        {
            var request = LockTryLockCodec.EncodeRequest(GetName(), ThreadUtil.GetThreadId(), long.MaxValue,
                GetTimeInMillis(time, unit), _lockReferenceIdGenerator.GetNextReferenceId());
            return Invoke(request, m => LockTryLockCodec.DecodeResponse(m).response);
        }

        public virtual void Unlock()
        {
            var request = LockUnlockCodec.EncodeRequest(GetName(), ThreadUtil.GetThreadId(), _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request);
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetKeyData());
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _lockReferenceIdGenerator = GetContext().GetClient().GetLockReferenceIdGenerator();
        }


        private IData GetKeyData()
        {
            if (_key == null)
            {
                _key = ToData(GetName());
            }
            return _key;
        }

        private long GetTimeInMillis(long time, TimeUnit? timeunit)
        {
            return timeunit.HasValue ? timeunit.Value.ToMillis(time) : time;
        }
    }
}