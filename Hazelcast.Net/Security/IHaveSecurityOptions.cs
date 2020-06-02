namespace Hazelcast.Security
{
    /// <summary>
    /// Defines options that expose security options.
    /// </summary>
    public interface IHaveSecurityOptions
    {
        /// <summary>
        /// Gets the security options.
        /// </summary>
        SecurityOptions Security { get; }
    }
}
