using System.Net;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{
    /// <summary>ICluster member interface.</summary>
    /// <remarks>
    ///     ICluster member interface. The default implementation
    ///     <see cref="Member">Member</see>
    ///     violates the Java Serialization contract.
    ///     It should be serialized/deserialized by Hazelcast.
    /// </remarks>
    /// <seealso cref="ICluster">ICluster</seealso>
    /// <seealso cref="IMembershipListener">IMembershipListener</seealso>
    public interface IMember : IDataSerializable, IEndpoint
    {
        /// <summary>Returns if this member is the local member.</summary>
        /// <remarks>Returns if this member is the local member.</remarks>
        /// <returns>
        ///     <tt>
        ///         true
        ///         <tt>
        ///             if this member is the
        ///             local member, <tt>false</tt> otherwise.
        /// </returns>
        bool LocalMember();

        //    /**
        //     * Returns the InetSocketAddress of this member.
        //     *
        //     * @return InetSocketAddress of this member
        //     *
        //     * @deprecated use {@link #getSocketAddress()} instead
        //     */
        //    @Deprecated
        //    InetSocketAddress getInetSocketAddress();
        /// <summary>Returns the socket address of this member.</summary>
        /// <remarks>Returns the socket address of this member.</remarks>
        /// <returns>socket address of this member</returns>
        IPEndPoint GetSocketAddress();

        /// <summary>Returns UUID of this member.</summary>
        /// <remarks>Returns UUID of this member.</remarks>
        /// <returns>UUID of this member.</returns>
        string GetUuid();

        Address GetAddress();
    }
}