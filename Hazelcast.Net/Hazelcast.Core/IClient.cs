using System.Net;

namespace Hazelcast.Core
{
    /// <summary>
    ///     IClient interface allows to get information about
    ///     a connected clients socket address, type and uuid.
    /// </summary>
    /// <remarks>
    ///     IClient interface allows to get information about
    ///     a connected clients socket address, type and uuid.
    /// </remarks>
    /// <seealso cref="IClientService">IClientService</seealso>
    /// <seealso cref="IClientListener">IClientListener</seealso>
    public interface IClient : IEndpoint
    {
        /// <summary>Returns unique uuid for this client</summary>
        /// <returns>unique uuid for this client</returns>
        string GetUuid();

        /// <summary>Returns socket address of this client</summary>
        /// <returns>socket address of this client</returns>
        IPEndPoint GetSocketAddress();

        /// <summary>Returns type of this client</summary>
        /// <returns>type of this client</returns>
        ClientType GetClientType();
    }
}