namespace Hazelcast.Clustering
{
    /// <summary>
    /// Defines the cluster states.
    /// </summary>
    public enum ClusterState
    {
        /// <summary>
        /// Unknown (default).
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The cluster has not connected yet.
        /// </summary>
        NotConnected,

        /// <summary>
        /// The cluster is connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// The cluster is connected.
        /// </summary>
        Connected,

        /// <summary>
        /// The cluster has disconnected and will not reconnect.
        /// </summary>
        Disconnected
    }
}
