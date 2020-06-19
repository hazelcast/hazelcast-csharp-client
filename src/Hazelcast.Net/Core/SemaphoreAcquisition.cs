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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a semaphore acquisition.
    /// </summary>
#pragma warning disable CA1815 // Override equals and operator equals on value types - not meant to be compared
    public struct SemaphoreAcquisition : IDisposable
#pragma warning restore CA1815
    {
        private readonly SemaphoreSlim _semaphore;
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreAcquisition"/> class.
        /// </summary>
        /// <param name="semaphore">A semaphore.</param>
        /// <param name="disposed">The dispose flag.</param>
        private SemaphoreAcquisition(SemaphoreSlim semaphore, int disposed = 0)
        {
            _semaphore = semaphore;
            _disposed = disposed;
        }

        private static readonly SemaphoreAcquisition NotAcquired = new SemaphoreAcquisition(null, 1);

        internal static SemaphoreAcquisition Create(Task task, SemaphoreSlim semaphore)
            => task.IsCompletedSuccessfully() ? new SemaphoreAcquisition(semaphore) : NotAcquired;

        internal static SemaphoreAcquisition Create(Task<bool> task, SemaphoreSlim semaphore)
            => task.IsCompletedSuccessfully() && task.Result ? new SemaphoreAcquisition(semaphore) : NotAcquired;

        /// <summary>
        /// Whether the lock was acquired.
        /// </summary>
        public bool Acquired => _semaphore != null;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            _semaphore?.Release();
        }

        /// <summary>
        /// Implicitly convert a <see cref="SemaphoreAcquisition"/> into a boolean.
        /// </summary>
        /// <param name="acquisition">The acquisition.</param>
        public static implicit operator bool(SemaphoreAcquisition acquisition)
            => acquisition.Acquired;
    }
}
