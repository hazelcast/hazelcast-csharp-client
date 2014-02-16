using System.Collections.Generic;
using System.Net;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{

    /// <summary>Cluster member interface.</summary>
    /// <remarks>
    /// Cluster member interface. The default implementation
    /// <see cref="com.hazelcast.instance.MemberImpl">com.hazelcast.instance.MemberImpl</see>
    /// violates the Java Serialization contract.
    /// It should be serialized/deserialized by Hazelcast.
    /// </remarks>
    /// <seealso cref="ICluster">Cluster</seealso>
    /// <seealso cref="IMembershipListener">MembershipListener</seealso>
    public interface IMember : IDataSerializable, IEndpoint
    {
        Address GetAddress();

        /// <summary>Returns if this member is the local member.</summary>
        /// <remarks>Returns if this member is the local member.</remarks>
        /// <returns>
        /// <tt>true<tt> if this member is the
        /// local member, <tt>false</tt> otherwise.
        /// </returns>
        bool LocalMember();

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
        IDictionary<string, object> GetAttributes();

        T GetAttribute<T>(string key);

        void SetAttribute<T>(string key, T value);


        /// <summary>
        /// Removes a key-value pair attribute for this member if given key was
        /// previously assigned as an attribute.<br/>
        /// If key wasn't assigned to a value this method does nothing.
        /// </summary>
        /// <remarks>
        /// Removes a key-value pair attribute for this member if given key was
        /// previously assigned as an attribute.<br/>
        /// If key wasn't assigned to a value this method does nothing.
        /// </remarks>
        /// <param name="key">The key to be deleted from the member attributes</param>
        /// <since>3.2</since>
        void RemoveAttribute(string key);
    }
}