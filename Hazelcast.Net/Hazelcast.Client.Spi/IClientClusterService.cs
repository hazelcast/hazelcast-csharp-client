using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    /// <author>mdogan 5/16/13</author>
    public interface IClientClusterService
    {
        /// <returns>The client interface representing the local client.</returns>
        IClient GetLocalClient();

        /// <summary>Gets the member for the given address.</summary>
        /// <param name="address">The address of the member to look up.</param>
        /// <returns>The member that was found, or null if not found. If address is null, null is returned.</returns>
        IMember GetMember(Address address);

        /// <summary>Gets the member with the given uuid.</summary>
        /// <param name="uuid">The uuid of the member.</param>
        /// <returns>The member that was found, or null if not found. If uuid is null, null is returned.</returns>
        IMember GetMember(string uuid);

        /// <summary>Gets the collection of members.</summary>
        /// <returns>The collection of members. Null will never be returned.</returns>
        ICollection<IMember> GetMemberList();

        /// <summary>Returns the address of the master member.</summary>
        /// <returns>The address of the master member. Could be null if the master is not yet known.</returns>
        Address GetMasterAddress();

        /// <summary>Gets the current number of members.</summary>
        /// <returns>The current number of members.</returns>
        int GetSize();

        /// <summary>Returns the cluster-time.</summary>
        /// <remarks>
        /// Returns the cluster-time.
        /// <p/>
        /// </remarks>
        /// <returns>The cluster-time.</returns>
        long GetClusterTime();

        /// <param name="listener">The listener to be registered.</param>
        /// <returns>The registration id.</returns>
        string AddMembershipListener(IMembershipListener listener);

        /// <param name="registrationId">The registrationId of the listener to be removed.</param>
        /// <returns>true if successfully removed, false otherwise.</returns>
        bool RemoveMembershipListener(string registrationId);

        /// <summary>The owner connection is opened to owner member of the client in the cluster.</summary>
        /// <remarks>
        /// The owner connection is opened to owner member of the client in the cluster.
        /// If the owner member dies, other members of the cluster assumes this client is dead.
        /// </remarks>
        /// <returns>The address of the owner connection.</returns>
        Address GetOwnerConnectionAddress();
    }
}
