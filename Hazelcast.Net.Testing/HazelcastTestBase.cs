namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests.
    /// </summary>
    public abstract class HazelcastTestBase
    {
        /// <summary>
        /// Provides assertions.
        /// </summary>
        protected Asserter Assert { get; } = new Asserter();
    }

    /// <summary>
    /// Provides assertions.
    /// </summary>
    public class Asserter
    { }
}
