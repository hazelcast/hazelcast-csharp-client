using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     IClientService allows to query connected
    ///     <see cref="IClient">IClient</see>
    ///     s and
    ///     attach/detach
    ///     <see cref="IClientListener">IClientListener</see>
    ///     s to listen connection events.
    /// </summary>
    /// <seealso cref="IClient">IClient</seealso>
    /// <seealso cref="IClientListener">IClientListener</seealso>
    public interface IClientService
    {
        /// <summary>Returns all connected clients to this member.</summary>
        /// <remarks>Returns all connected clients to this member.</remarks>
        /// <returns>all connected clients to this member.</returns>
        ICollection<IClient> GetConnectedClients();

        /// <param name="clientListener">IClientListener</param>
        /// <returns>returns registration id.</returns>
        string AddClientListener(IClientListener clientListener);

        /// <param name="registrationId">Id of listener registration.</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveClientListener(string registrationId);
    }
}