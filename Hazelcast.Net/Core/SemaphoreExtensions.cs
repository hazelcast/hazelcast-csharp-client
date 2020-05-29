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

using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    public static class SemaphoreExtensions
    {
        /// <summary>
        /// Acquires a semaphore.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <returns>A <see cref="SemaphoreAcquisition"/> instance that needs to be disposed to release the semaphore.</returns>
        public static
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            ValueTask<SemaphoreAcquisition> AcquireAsync(this SemaphoreSlim semaphore)
        {
            var task = semaphore
                .WaitAsync()
                .ContinueWith(x => SemaphoreAcquisition.Create(x, semaphore),
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Acquires a semaphore.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="SemaphoreAcquisition"/> instance that needs to be disposed to release the semaphore.</returns>
        public static
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<SemaphoreAcquisition> AcquireAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            var task = semaphore
                .WaitAsync(cancellationToken)
                .ContinueWith(x => SemaphoreAcquisition.Create(x, semaphore),
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Tries to acquire a semaphore immediately.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <returns>A <see cref="SemaphoreAcquisition"/> instance that needs to be disposed to release the semaphore.</returns>
        public static
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<SemaphoreAcquisition> TryAcquireAsync(this SemaphoreSlim semaphore)
        {
            var task = semaphore
                .WaitAsync(0)
                .ContinueWith(x => SemaphoreAcquisition.Create(x, semaphore),
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }
    }
}
