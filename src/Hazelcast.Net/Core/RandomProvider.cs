// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides safe random numbers.
    /// </summary>
    internal static class RandomProvider
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
#pragma warning disable CA5394 // Do not use insecure randomness
            lock (GlobalLock) return new Random(GlobalRandom.Next());
#pragma warning restore CA5394
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

        /// <summary>
        /// Returns a non-negative random integer number (thread-safe, not appropriate for security purposes).
        /// </summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.</returns>
        /// <remarks>
        /// <para>Using this method will not trigger a CA5394 "Do not use insecure randomness" warning,
        /// yet this method should NOT be used for anything related to security.</para>
        /// </remarks>
#pragma warning disable CA5394 // Do not use insecure randomness
        public static int Next() => Random.Next();
#pragma warning restore CA5394

        /// <summary>
        /// Returns a non-negative random integer number that is less than the specified maximum (thread-safe, not appropriate for security purposes).
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <remarks>
        /// <para>Using this method will not trigger a CA5394 "Do not use insecure randomness" warning,
        /// yet this method should NOT be used for anything related to security.</para>
        /// </remarks>
#pragma warning disable CA5394 // Do not use insecure randomness
        public static int Next(int maxValue) => Random.Next(maxValue);
#pragma warning restore CA5394

        /// <summary>
        /// Returns a non-negative random integer number that is within the specified range (thread-safe, not appropriate for security purposes).
        /// </summary>
        /// <param name="minValue">The non-negative, inclusive lower bound of the random number to be generated.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A 32-bit signed integer that is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <remarks>
        /// <para>Using this method will not trigger a CA5394 "Do not use insecure randomness" warning,
        /// yet this method should NOT be used for anything related to security.</para>
        /// </remarks>
#pragma warning disable CA5394 // Do not use insecure randomness
        public static int Next(int minValue, int maxValue) => Random.Next(minValue, maxValue);
#pragma warning restore CA5394

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0 (thread-safe, not appropriate for security purposes).
        /// </summary>
        /// <returns>A double-precision floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        /// <remarks>
        /// <para>Using this method will not trigger a CA5394 "Do not use insecure randomness" warning,
        /// yet this method should NOT be used for anything related to security.</para>
        /// </remarks>
#pragma warning disable CA5394 // Do not use insecure randomness
        public static double NextDouble() => Random.NextDouble();
#pragma warning restore CA5394
    }
}
