﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;

namespace Hazelcast.CP
{
    public interface IFencedLock : ICPDistributedObject
    {
        long InvalidFence { get; }

        ICPGroupId CPGroupId { get; }

        Task LockAsync();

        Task LockInterruptiblyAsync();

        Task<long> LockAndGetFenceAsync();

        Task<bool> TryLockAsync(TimeSpan time);

        Task<long> TryLockAndGetFenceAsync(TimeSpan time);

        Task UnlockAsync();

        Task<bool> IsLockedAsync();

        Task<long> GetFenceAsync();

        Task<int> GetLockCountAasync();

        Task<bool> IsLockedByCurrentThreadAsync();// todo: check naming the thread
    }
}