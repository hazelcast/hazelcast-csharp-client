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
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides safe random numbers.
    /// </summary>
    [SuppressMessage("NDepend", "ND3101:DontUseSystemRandomForSecurityPurposes", Justification = "No security here.")]
    public static class RandomProvider
    {
        // Notes
        //
        // Random is *not* thread-safe
        // read
        //   https://stackoverflow.com/questions/3049467
        //   https://docs.microsoft.com/en-us/dotnet/api/system.random
        //   https://codeblog.jonskeet.uk/2009/11/04/revisiting-randomness/
        // for best-practices.

        private static readonly Random GlobalRandom = new Random();
        private static readonly object GlobalLock = new object();
        private static readonly ThreadLocal<Random> ThreadRandom = new ThreadLocal<Random>(NewRandom);

        /// <summary>
        /// Creates a new random for a thread.
        /// </summary>
        /// <returns>A new random for a thread.</returns>
        private static Random NewRandom()
        {
            // use GlobalRandom to get a random seed, using GlobalLock
            // because the Random class is not thread safe
            lock (GlobalLock) return new Random(GlobalRandom.Next());
        }

        /// <summary>
        /// Gets a thread-safe <see cref="Random"/> instance (do *not* cache this instance).
        /// </summary>
        /// <remarks>
        /// <para>The instance is thread-safe because it is local to the thread. Do *not*
        /// store the instance in a variable as that may break thread safety. Instead,
        /// retrieve it each time it is required.</para>
        /// </remarks>
        public static Random Random => ThreadRandom.Value;
    }
}
