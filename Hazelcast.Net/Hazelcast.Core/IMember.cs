using System.Collections.Generic;
using System.Net;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{

    /// <summary>Cluster member interface.</summary>
    /// <remarks>
    /// Cluster member interface. The default implementation violates the Serialization contract.
    /// It should be serialized/deserialized by Hazelcast.
    /// </remarks>
    /// <seealso cref="ICluster">Cluster</seealso>
    /// <seealso cref="IMembershipListener">MembershipListener</seealso>
    public interface IMember : IEndpoint
    {
        Address GetAddress();

        /// <summary>Returns the socket address of this member.</summary>
        /// <remarks>Returns the socket address of this member.</remarks>
        /// <returns>socket address of this member</returns>
        IPEndPoint GetSocketAddress();

        /// <summary>Returns UUID of this member.</summary>
        /// <remarks>Returns UUID of this member.</remarks>
        /// <returns>UUID of this member.</returns>
        string GetUuid();

        /// <summary>
        /// Returns configured attributes for this member.<br/>
        /// <b>This method might not be available on all native clients.</b>
        /// </summary>
        /// <returns>Attributes for this member.</returns>
        /// <since>3.2</since>
        IDictionary<string, string> GetAttributes();

        string GetAttribute(string key);

        // <summary>Returns true if this member is a lite member.</summary>
        /// <returns>
        /// <tt>true</tt> if this member is a lite member, <tt>false</tt> otherwise.
        /// Lite members do not own any partition.
        /// </returns>
        bool IsLiteMember { get; }
    }
}