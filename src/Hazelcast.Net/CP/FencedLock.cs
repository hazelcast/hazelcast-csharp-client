using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hazelcast.CP
{
    internal class FencedLock : CPDistributedObjectBase, IFencedLock
    {
        public long InvalidFence => throw new NotImplementedException();

        public ICPGroupId CPGroupId => throw new NotImplementedException();

        public ICPGroupId GroupId => throw new NotImplementedException();

        public string ServiceName => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string PartitionKey => throw new NotImplementedException();

        public ValueTask DestroyAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<long> GetFenceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetLockCountAasync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsLockedAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsLockedByCurrentThreadAsync()
        {
            throw new NotImplementedException();
        }

        public Task<long> LockAndGetFenceAsync()
        {
            throw new NotImplementedException();
        }

        public Task LockAsync()
        {
            throw new NotImplementedException();
        }

        public Task LockInterruptiblyAsync()
        {
            throw new NotImplementedException();
        }

        public Task<long> TryLockAndGetFenceAsync(TimeSpan time)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryLockAsync(TimeSpan time)
        {
            throw new NotImplementedException();
        }

        public Task UnlockAsync()
        {
            throw new NotImplementedException();
        }
    }
}
