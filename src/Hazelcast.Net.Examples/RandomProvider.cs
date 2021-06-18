using System;
using System.Threading;

namespace Hazelcast.Examples
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
            lock (GlobalLock) return new Random(GlobalRandom.Next());
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
    }
}