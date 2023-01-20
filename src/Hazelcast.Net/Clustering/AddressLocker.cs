// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    internal class AddressLocker
    {
        private readonly Dictionary<NetworkAddress, LockInfo> _locks
            = new Dictionary<NetworkAddress, LockInfo>();

        private readonly object _lock = new object();

        private class LockInfo : IDisposable
        {
            private readonly AddressLocker _addressLocker;

            public LockInfo(AddressLocker addressLocker, NetworkAddress address)
            {
                _addressLocker = addressLocker;

                Address = address;
                Semaphore = new SemaphoreSlim(1);
            }

            public NetworkAddress Address { get; }

            public int ReferenceCount { get; set; }

            public SemaphoreSlim Semaphore { get; }

            public void Dispose()
            {
                Semaphore.Release();
                _addressLocker.Return(this);
            }
        }

        private LockInfo GetLockInfo(NetworkAddress address)
        {
            LockInfo lockInfo;

            lock (_lock)
            {
                if (!_locks.TryGetValue(address, out lockInfo))
                    lockInfo = _locks[address] = new LockInfo(this, address);
                lockInfo.ReferenceCount++;
            }

            return lockInfo;
        }

        private void Return(LockInfo lockInfo)
        {
            lock (_lock)
            {
                lockInfo.ReferenceCount--;
                if (lockInfo.ReferenceCount == 0)
                    _locks.Remove(lockInfo.Address);
            }
        }

        public async Task<IDisposable> LockAsync(NetworkAddress address)
        {
            var lockInfo = GetLockInfo(address);
            await lockInfo.Semaphore.WaitAsync().CfAwait();
            return lockInfo;
        }
    }
}
