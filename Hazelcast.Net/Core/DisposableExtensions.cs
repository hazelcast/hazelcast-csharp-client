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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="IDisposable"/> interface.
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Tries to dispose an <see cref="IDisposable"/> without throwing.
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        /// <param name="logger">An optional logger.</param>
        public static void TryDispose(this IDisposable disposable, ILogger logger = null)
        {
            if (disposable == null) return;

            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Caught an exception while disposing {disposable.GetType()}.");
            }
        }

        /// <summary>
        /// Tries to dispose an <see cref="IAsyncDisposable"/> without throwing.
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        /// <param name="logger">An optional logger.</param>
        /// <returns>A task that completes when the disposable has been disposed.</returns>
        public static async ValueTask TryDisposeAsync(this IAsyncDisposable disposable, ILogger logger = null)
        {
            if (disposable == null) return;

            try
            {
                await disposable.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Caught an exception while disposing {disposable.GetType()}.");
            }
        }
    }
}
