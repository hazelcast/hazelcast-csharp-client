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
using System.Collections.Generic;
using Hazelcast.Core;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Compares <see cref="NearCacheEntry"/> using the default comparison.
    /// </summary>
    internal class DefaultComparer : IComparer<AsyncLazy<NearCacheEntry>>
    {
        /// <inheritdoc />
        public int Compare(AsyncLazy<NearCacheEntry> x, AsyncLazy<NearCacheEntry> y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));

            var cx = x.Value.Key.GetHashCode();
            var cy = y.Value.Key.GetHashCode();

            return cx.CompareTo(cy);
        }
    }
}
