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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Core
{
    internal static class LinqExtensions
    {
        public static IEnumerable<KeyValuePair<TKey, TValue>> WherePair<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, bool> predicate)
            => source.Where(pair => predicate(pair.Key, pair.Value));

        public static IEnumerable<KeyValuePair<TKey, TValue>> WherePair<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, int, bool> predicate)
            => source.Where((pair, index) => predicate(pair.Key, pair.Value, index));

        public static IEnumerable<TResult> SelectPair<TKey, TValue, TResult>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, TResult> selector)
            => source.Select(pair => selector(pair.Key, pair.Value));

        public static IEnumerable<TResult> SelectPair<TKey, TValue, TResult>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, int, TResult> selector)
            => source.Select((pair, index) => selector(pair.Key, pair.Value, index));
    }
}
