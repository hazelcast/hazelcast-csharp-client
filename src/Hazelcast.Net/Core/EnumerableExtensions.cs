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
using System.Linq;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="IEnumerable{T}"/> interface.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Shuffles an enumerable.
        /// </summary>
        /// <typeparam name="T">The enumerated type.</typeparam>
        /// <param name="source">The original enumerable.</param>
        /// <returns>The original enumerable items, in random order.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
            => source.OrderBy(x => RandomProvider.Random.Next());

        /// <summary>
        /// Combine <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T1">The first enumerated type.</typeparam>
        /// <typeparam name="T2">The second enumerated type.</typeparam>
        /// <typeparam name="T3">The third enumerated type.</typeparam>
        /// <typeparam name="T4">The fourth enumerated type.</typeparam>
        /// <param name="t1">The first <see cref="IEnumerable{T}"/>.</param>
        /// <param name="t2">The second <see cref="IEnumerable{T}"/>.</param>
        /// <param name="t3">The third <see cref="IEnumerable{T}"/>.</param>
        /// <param name="t4">The fourth <see cref="IEnumerable{T}"/>.</param>
        /// <returns></returns>
        public static IEnumerable<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(IEnumerable<T1> t1, IEnumerable<T2> t2, IEnumerable<T3> t3, IEnumerable<T4> t4)
        {
            if (t1 == null) throw new ArgumentNullException(nameof(t1));
            if (t2 == null) throw new ArgumentNullException(nameof(t2));
            if (t3 == null) throw new ArgumentNullException(nameof(t3));
            if (t4 == null) throw new ArgumentNullException(nameof(t4));

            var i1 = t1.GetEnumerator();
            var i2 = t2.GetEnumerator();
            var i3 = t3.GetEnumerator();
            var i4 = t4.GetEnumerator();

            try
            {
                while (i1.MoveNext() && i4.MoveNext() && i3.MoveNext() && i2.MoveNext())
                    yield return (i1.Current, i2.Current, i3.Current, i4.Current);
            }
            finally
            {
                i1.Dispose();
                i2.Dispose();
                i3.Dispose();
                i4.Dispose();
            }
        }
    }
}
