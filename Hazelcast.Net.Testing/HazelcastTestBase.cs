using System;
using Hazelcast.Core;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests.
    /// </summary>
    public abstract class HazelcastTestBase
    {
        private static readonly ISequence<int> UniqueNameSequence = new Int32Sequence();
        private static readonly string UniqueNamePrefix = DateTime.Now.ToString("HHmmss_");

        /// <summary>
        /// Gets the default test timeout in milliseconds.
        /// </summary>
        public const int TimeoutMilliseconds = 20_000;

        /// <summary>
        /// Provides assertions.
        /// </summary>
        protected Asserter Assert { get; } = new Asserter();

        /// <summary>
        /// Creates a unique name.
        /// </summary>
        /// <returns>A unique name.</returns>
        /// <remarks>
        /// <para>The unique name is unique across all tests within a run, and to
        /// a certain extend also unique across all runs.</para>
        /// </remarks>
        protected string CreateUniqueName() => UniqueNamePrefix + UniqueNameSequence.Next;
    }
}
