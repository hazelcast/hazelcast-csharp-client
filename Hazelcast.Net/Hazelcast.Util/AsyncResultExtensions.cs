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

namespace Hazelcast.Util
{
    /// <summary>
    /// Provides extension methods for the <see cref="IAsyncResult"/> interface.
    /// </summary>
    internal static class AsyncResultExtensions
    {
        /// <summary>
        /// Blocks the current thread until the <see cref="IAsyncResult"/> completes.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/>.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <c>Infinite</c> (-1) to wait indefinitely.</param>
        /// <param name="exitContext"><c>true</c> to exit the synchronization context</param>
        /// <returns><c>true</c> if the <see cref="IAsyncResult"/> completes; otherwise, <c>false</c>.</returns>
        public static bool Wait(this IAsyncResult result, int millisecondsTimeout, bool exitContext = true)
        {
            return result.AsyncWaitHandle.WaitOne(millisecondsTimeout, exitContext);
        }
    }
}