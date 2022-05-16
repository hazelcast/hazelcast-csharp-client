// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Runtime.CompilerServices;
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="int"/> value type.
    /// </summary>
    internal static class Int32Extensions
    {
        /// <summary>
        /// Compares the value to zero and, if equal, replaces with one.
        /// </summary>
        /// <param name="value">The value to compare and replace.</param>
        /// <returns><c>true</c> if the value was replaced; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InterlockedZeroToOne(this ref int value)
        {
            return Interlocked.CompareExchange(ref value, 1, 0) == 0;
        }
    }
}
