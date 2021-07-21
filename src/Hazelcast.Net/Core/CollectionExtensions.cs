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
using System.Collections.ObjectModel;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.Core
{
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Removes the first occurrence of a specific key-value pair from a collection of key-value pairs.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if an occurrence was removed; otherwise <c>false</c>.</returns>
        public static bool TryRemove<TKey, TValue>(this ICollection<KeyValuePair<TKey, TValue>> source, TKey key, TValue value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Remove(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Returns read-only wrapper for <paramref name="list"/>
        /// or <paramref name="list"/> itself if it already implements <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> list) => list as IReadOnlyList<T> ?? new ReadOnlyCollection<T>(list);

        /// <summary>
        /// Returns read-only wrapper for <paramref name="list"/> where each element is returned as <see cref="object"/> with boxing performed if needed
        /// or <paramref name="list"/> itself if it already implements <see cref="IReadOnlyList{T}"/> (T is <see cref="object"/>).
        /// </summary>
        public static IReadOnlyList<object> AsReadOnlyObjectList<T>(this IList<T> list) => list as IReadOnlyList<object> ?? new ReadOnlyObjectList<T>(list);
    }
}
