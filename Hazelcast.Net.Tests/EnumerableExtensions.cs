using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;

namespace AsyncTests1
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
    }
}
