using System;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientLockProxy : ClientProxy, ILock
    {
        private volatile IData key;

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
                ThreadUtil.GetThreadId());
            Invoke(request);            
        }

        public virtual void ForceUnlock()
        {
            var request = LockForceUnlockCodec.EncodeRequest(GetName());
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
                GetTimeInMillis(time, unit));
            return Invoke(request, m => LockTryLockCodec.DecodeResponse(m).response);          
        }

        public virtual void Unlock()
        {
            var request = LockUnlockCodec.EncodeRequest(GetName(), ThreadUtil.GetThreadId());
            Invoke(request);
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetKeyData());
        }

        private IData GetKeyData()
        {
            if (key == null)
            {
                key = ToData(GetName());
            }
            return key;
        }

        private long GetTimeInMillis(long time, TimeUnit? timeunit)
        {
            return timeunit.HasValue ? timeunit.Value.ToMillis(time) : time;
        }
    }
}