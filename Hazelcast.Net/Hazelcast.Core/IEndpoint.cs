using System.Net;

namespace Hazelcast.Core
{
    /// <summary>Endpoint represents a peer in the cluster.</summary>
    /// <remarks>
    ///     Endpoint represents a peer in the cluster.
    ///     It can be either a member or a client.
    /// </remarks>
    /// <seealso cref="IMember">IMember</seealso>
    /// <seealso cref="IClient">IClient</seealso>
    public interface IEndpoint
    {
        /// <summary>Returns unique uuid for this endpoint</summary>
        /// <returns>unique uuid for this endpoint</returns>
        string GetUuid();

        /// <summary>Returns socket address of this endpoint</summary>
        /// <returns>socket address of this endpoint</returns>
        IPEndPoint GetSocketAddress();
    }
}