using System;
using System.Collections.Generic;

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
    }
}
