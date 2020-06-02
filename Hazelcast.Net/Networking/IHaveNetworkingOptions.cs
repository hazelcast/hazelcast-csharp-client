namespace Hazelcast.Networking
{
    /// <summary>
    /// Defines options that expose networking options.
    /// </summary>
    public interface IHaveNetworkingOptions
    {
        /// <summary>
        /// Gets the networking options.
        /// </summary>
        NetworkingOptions Network { get; }
    }
}
