// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Shuffles an enumerable.
        /// </summary>
        /// <typeparam name="T">The enumerated type.</typeparam>
        /// <param name="source">The original enumerable.</param>
        /// <returns>The original items, in random order.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
            => source.OrderBy(x => RandomProvider.Next());

        /// <summary>
        /// Shuffles a collection.
        /// </summary>
        /// <typeparam name="T">The enumerated type.</typeparam>
        /// <param name="source">The original collection.</param>
        /// <returns>The original items, in random order.</returns>
        public static IReadOnlyCollection<T> Shuffle<T>(this IReadOnlyCollection<T> source)
        {
            // if source is a collection, we can optimize the list creation
            var l = new List<T>(source.Count);
            l.AddRange(source.OrderBy(x => RandomProvider.Next()));
            return l;
        }

        /// <summary>
        /// Combine multiple <see cref="IEnumerable{T}"/> instances.
        /// </summary>
        /// <typeparam name="T1">The first enumerated type.</typeparam>
        /// <typeparam name="T2">The second enumerated type.</typeparam>
        /// <param name="source">The instances to combine.</param>
        /// <returns>One single <see cref="IEnumerable{T}"/> combining the multiple instances.</returns>
        public static IEnumerable<(T1, T2)> Combine<T1, T2>(this (IEnumerable<T1> Source1, IEnumerable<T2> Source2) source)
        {
            var (source1, source2) = source;

            if (source1 == null) throw new ArgumentException("Element #1 of source is null.", nameof(source));
            if (source2 == null) throw new ArgumentException("Element #2 of source is null.", nameof(source));

            var i1 = source1.GetEnumerator();
            var i2 = source2.GetEnumerator();

            try
            {
                while (i1.MoveNext() && i2.MoveNext())
                    yield return (i1.Current, i2.Current);
            }
            finally
            {
                i1.Dispose();
                i2.Dispose();
            }
        }

        /// <summary>
        /// Combine <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T1">The first enumerated type.</typeparam>
        /// <typeparam name="T2">The second enumerated type.</typeparam>
        /// <typeparam name="T3">The third enumerated type.</typeparam>
        /// <typeparam name="T4">The fourth enumerated type.</typeparam>
        /// <param name="source">The instances to combine.</param>
        /// <returns>One single <see cref="IEnumerable{T}"/> combining the multiple instances.</returns>
        public static IEnumerable<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(this (IEnumerable<T1> Source1, IEnumerable<T2> Source2, IEnumerable<T3> Source3, IEnumerable<T4> Source4) source)
        {
            var (source1, source2, source3, source4) = source;

            if (source1 == null) throw new ArgumentException("Element #1 of source is null.", nameof(source));
            if (source2 == null) throw new ArgumentException("Element #2 of source is null.", nameof(source));
            if (source3 == null) throw new ArgumentException("Element #3 of source is null.", nameof(source));
            if (source4 == null) throw new ArgumentException("Element #4 of source is null.", nameof(source));

            var i1 = source1.GetEnumerator();
            var i2 = source2.GetEnumerator();
            var i3 = source3.GetEnumerator();
            var i4 = source4.GetEnumerator();

            try
            {
                while (i1.MoveNext() && i2.MoveNext() && i3.MoveNext() && i4.MoveNext())
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

#if !NET8_0_OR_GREATER
        /// <summary>
        /// Creates a <see cref="Dictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="IDictionary{TKey,TValue}"/> from.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> that contains values provided by <paramref name="source"/>.</returns>
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
            => source.ToDictionary(x => x.Key, x => x.Value);

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/> of (key, value) pairs.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="IDictionary{TKey,TValue}"/> from.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> that contains values provided by <paramref name="source"/>.</returns>
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<(TKey Key, TValue Value)> source)
            => source.ToDictionary(x => x.Key, x => x.Value);
#endif

        /// <summary>
        /// Enumerates <paramref name="source"/> to a new <see cref="List{T}"/> starting from <paramref name="initialCapacity"/> size.
        /// This allows to avoid or minimize list resizing if number of elements is known fully or approximately.
        /// </summary>
        public static List<T> ToList<T>(this IEnumerable<T> source, int initialCapacity)
        {
            var list = new List<T>(initialCapacity);
            list.AddRange(source);
            return list;
        }

        /// <summary>
        /// Deconstructs an <see cref="IEnumerable{T}"/> into its items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to deconstruct.</param>
        /// <param name="item1">The first item.</param>
        public static void Deconstruct<T>(this IEnumerable<T> source, out T item1)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            using var e = source.GetEnumerator();
            if (!e.MoveNext()) throw new ArgumentException("Source does not contain enough items.", nameof(source));
            item1 = e.Current;
        }

        /// <summary>
        /// Deconstructs an <see cref="IEnumerable{T}"/> into its items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to deconstruct.</param>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        public static void Deconstruct<T>(this IEnumerable<T> source, out T item1, out T item2)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            using var e = source.GetEnumerator();
            if (!e.MoveNext()) throw new ArgumentException("Source does not contain enough items.", nameof(source));
            item1 = e.Current;
            if (!e.MoveNext()) throw new ArgumentException("Source does not contain enough items.", nameof(source));
            item2 = e.Current;
        }

        /// <summary>
        /// Gets the index of the first <see cref="IList{T}"/> item that satisfies the specified <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="list">A list of items.</param>
        /// <param name="predicate">A predicate.</param>
        /// <returns>The index of the first item that satisfies the specified <paramref name="predicate"/>, or -1.</returns>
        public static int IndexOf<T>(this IList<T> list, Func<T, bool> predicate)
        {
            var i = 0;
            foreach (var item in list)
                if (predicate(item))
                    return i;
                else
                    i += 1;
            return -1;
        }

        /// <summary>
        /// Gets the index of the last <see cref="IList{T}"/> item that satisfies the specified <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="list">A list of items.</param>
        /// <param name="predicate">A predicate.</param>
        /// <returns>The index of the last item that satisfies the specified <paramref name="predicate"/>, or -1.</returns>
        public static int LastIndexOf<T>(this IList<T> list, Func<T, bool> predicate)
        {
            var i = 0;
            var f = -1;
            foreach (var item in list)
            {
                if (predicate(item)) f = i;
                i += 1;
            }
            return f;
        }

        /// <summary>
        /// Maps a list.
        /// </summary>
        /// <typeparam name="T">The source type.</typeparam>
        /// <typeparam name="R">The destination type.</typeparam>
        /// <param name="list">The source list.</param>
        /// <param name="map">The mapping function.</param>
        /// <returns>The mapped list.</returns>
        public static List<R> Map<T, R>(this IList<T> list, Func<T, R> map)
        {
            var result = new List<R>(list.Count);
            result.AddRange(list.Select(map));
            return result;
        }

        /// <summary>
        /// Filters a sequence of <see cref="KeyValuePair{TKey,TValue}"/> based on a predicate.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">A sequence of <see cref="KeyValuePair{TKey,TValue}"/>.</param>
        /// <param name="predicate">A function to test each <see cref="KeyValuePair{TKey,TValue}"/> for a condition.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/> from the input sequence that satisfy the condition.</returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> WherePair<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, bool> predicate)
            => source.Where(pair => predicate(pair.Key, pair.Value));

        /// <summary>
        /// Filters a sequence of <see cref="KeyValuePair{TKey,TValue}"/> based on a predicate.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">A sequence of <see cref="KeyValuePair{TKey,TValue}"/>.</param>
        /// <param name="predicate">A function to test each <see cref="KeyValuePair{TKey,TValue}"/> for a condition.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/> from the input sequence that satisfy the condition.</returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> WherePair<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, int, bool> predicate)
            => source.Where((pair, index) => predicate(pair.Key, pair.Value, index));

        /// <summary>
        /// Project each element of a sequence of <see cref="KeyValuePair{TKey,TValue}"/> into a new form.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type of the projected elements.</typeparam>
        /// <param name="source">A sequence of <see cref="KeyValuePair{TKey,TValue}"/>.</param>
        /// <param name="selector">A transform function to apply to each <see cref="KeyValuePair{TKey,TValue}"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/> whose elements are the result of the transform function on each element of the source.</returns>
        public static IEnumerable<TResult> SelectPair<TKey, TValue, TResult>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, TResult> selector)
            => source.Select(pair => selector(pair.Key, pair.Value));

        /// <summary>
        /// Project each element of a sequence of <see cref="KeyValuePair{TKey,TValue}"/> into a new form.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type of the projected elements.</typeparam>
        /// <param name="source">A sequence of <see cref="KeyValuePair{TKey,TValue}"/>.</param>
        /// <param name="selector">A transform function to apply to each <see cref="KeyValuePair{TKey,TValue}"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/> whose elements are the result of the transform function on each element of the source.</returns>
        public static IEnumerable<TResult> SelectPair<TKey, TValue, TResult>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, int, TResult> selector)
            => source.Select((pair, index) => selector(pair.Key, pair.Value, index));

        /// <summary>
        /// Adds an index to a sequence of values.
        /// </summary>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">A sequence of <typeparamref name="TValue"/>.</param>
        /// <returns>A sequence of <typeparamref name="TValue"/> with an index.</returns>
        public static IEnumerable<(TValue, int)> WithIndex<TValue>(this IEnumerable<TValue> source)
        {
            var i = 0;
            foreach (var item in source) yield return (item, i++);
        }
    }
}
