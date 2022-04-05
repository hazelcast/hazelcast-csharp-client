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

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;

namespace Hazelcast
{
    public static class CompactExtensions
    {
        /// <summary>
        /// Asynchronously gets the element at the specified index in the read-only list.
        /// </summary>
        /// <typeparam name="T">The type of the items in this list.</typeparam>
        /// <param name="list">This list.</param>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        /// <remarks>
        /// <para>Should the list be a lazy-deserialized list, and should the deserialization
        /// involve asynchronous communication with the cluster, this method will allow
        /// communication to take place, whereas the direct <c>list[index]</c> may result
        /// in an exception being thrown.</para>
        /// </remarks>
        public static ValueTask<T> GetItemAsync<T>(this IReadOnlyList<T> list, int index)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            return list is ReadOnlyLazyList<T> lazy
                ? lazy.GetItemAsync(index)
                : new ValueTask<T>(list[index]);
        }

        /// <summary>
        /// Gets an object that asynchronously enumerates the read-only list.
        /// </summary>
        /// <typeparam name="T">The type of the items in this list.</typeparam>
        /// <param name="list">This list.</param>
        /// <returns>An object that asynchronously enumerates the read-only list.</returns>
        /// <remarks>
        /// <para>Should the list be a lazy-deserialized list, and should the deserialization
        /// involve asynchronous communication with the cluster, this method will allow
        /// communication to take place, whereas the direct enumeration of the list may result
        /// in an exception being thrown.</para>
        /// </remarks>
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IReadOnlyList<T> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            return list is ReadOnlyLazyList<T> lazy
                ? (IAsyncEnumerable<T>) lazy
                : new AsyncEnumerableWrapper<T>(list);
        }

        /// <summary>
        /// Gets an object that asynchronously enumerates the read-only collection.
        /// </summary>
        /// <typeparam name="T">The type of the items in this collection.</typeparam>
        /// <param name="collection">This collection.</param>
        /// <returns>An object that asynchronously enumerates the read-only collection.</returns>
        /// <remarks>
        /// <para>Should the collection be a lazy-deserialized collection, and should the deserialization
        /// involve asynchronous communication with the cluster, this method will allow
        /// communication to take place, whereas the direct enumeration of the collection may result
        /// in an exception being thrown.</para>
        /// </remarks>
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IReadOnlyCollection<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection is ReadOnlyLazyList<T> lazy
                ? (IAsyncEnumerable<T>) lazy
                : new AsyncEnumerableWrapper<T>(collection);
        }

        // FIXME - now do all other lazy thing enumeration + test it all
        
        // asynchronously enumerates a synchronous enumerable
        private class AsyncEnumerableWrapper<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> _source;

            public AsyncEnumerableWrapper(IEnumerable<T> source)
            {
                _source = source;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
                => new AsyncEnumerator(_source.GetEnumerator());

            private class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private readonly IEnumerator<T> _source;

                public AsyncEnumerator(IEnumerator<T> source)
                {
                    _source = source;
                }

                public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_source.MoveNext());

                public T Current => _source.Current;

                public ValueTask DisposeAsync()
                {
                    _source.Dispose();
                    return default;
                }
            }
        }
    }
}
