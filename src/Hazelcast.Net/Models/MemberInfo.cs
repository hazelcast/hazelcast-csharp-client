// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Networking;

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents a member of a cluster.
    /// </summary>
    /// <remarks>
    /// <para>This class implements <see cref="IEquatable{MemberInfo}"/> and two instances are considered
    /// equal if their <see cref="Id"/> are identical (the other fields are not considered for equality).</para>
    /// </remarks>
    public class MemberInfo : IEquatable<MemberInfo>
    {
        private static readonly Dictionary<EndpointQualifier, NetworkAddress> EmptyAddressMap = new Dictionary<EndpointQualifier, NetworkAddress>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfo"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the member.</param>
        /// <param name="address">The network address of the member.</param>
        /// <param name="version">The version of the server running the member.</param>
        /// <param name="isLiteMember">Whether the member is a "lite" member.</param>
        /// <param name="attributes">Attributes of the member.</param>
        public MemberInfo(Guid id, NetworkAddress address, MemberVersion version, bool isLiteMember, IReadOnlyDictionary<string, string> attributes)
            : this(address, id, attributes, isLiteMember, version, false, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfo"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the member.</param>
        /// <param name="address">The network address of the member.</param>
        /// <param name="version">The version of the server running the member.</param>
        /// <param name="isLiteMember">Whether the member is a "lite" member.</param>
        /// <param name="attributes">Attributes of the member.</param>
        /// <param name="addressMapExists">Whether the address map exists.</param>
        /// <param name="addressMap">The address map.</param>
        /// <remarks>
        /// <para>That overload of the constructor is required by generated codecs.</para>
        /// </remarks>
        internal MemberInfo(NetworkAddress address, Guid id, IReadOnlyDictionary<string, string> attributes, bool isLiteMember, MemberVersion version, bool addressMapExists, IReadOnlyDictionary<EndpointQualifier, NetworkAddress> addressMap)
        {
            // yes, this constructor could be simplified, but it is used (exclusively) by the codec,
            // and must respect what the codec expects, so don't simplify it!

            Id = id;
            Address = address;
            Version = version;
            IsLiteMember = isLiteMember;
            Attributes = attributes;

            if (addressMapExists)
            {
                AddressMap = addressMap;
                PublicAddress = addressMap.WherePair((qualifier, _) => qualifier.Type == ProtocolType.Client && qualifier.Identifier == "public")
                    .SelectPair((_, addr) => addr)
                    .FirstOrDefault();
            }
            else
            {
                AddressMap = EmptyAddressMap; // will never get modified = safe
                PublicAddress = null;
            }
        }

        /// <summary>
        /// Whether to use the public address or the internal address to connect to the member.
        /// </summary>
        /// <remarks>Determines the value of <see cref="ConnectAddress"/>.</remarks>
        internal bool UsePublicAddress { get; set; }

        /// <summary>
        /// Gets the unique identifier of the member.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// (for internal use only) Gets the unique identifier of the member.
        /// </summary>
        /// <remarks>
        /// <para>Generated codecs expect this naming of the property. The public version
        /// of this is <see cref="Id"/>.</para>
        /// </remarks>
        internal Guid Uuid => Id;

        /// <summary>
        /// Gets the network address of the member.
        /// </summary>
        public NetworkAddress Address { get; }

        /// <summary>
        /// Gets the public network address of the member.
        /// </summary>
        public NetworkAddress PublicAddress { get; internal set; }

        /// <summary>
        /// Whether the member has a public address in addition to its address.
        /// </summary>
        public bool HasPublicAddress => PublicAddress != null;

        /// <summary>
        /// Gets the address to connect to.
        /// </summary>
        /// <remarks>The address to connect to is either the <see cref="PublicAddress"/> or the <see cref="Address"/>,
        /// depending on the network structure and how members can be reached by the client.</remarks>
        internal NetworkAddress ConnectAddress => UsePublicAddress ? PublicAddress : Address;

        /// <summary>
        /// Gets the version of the server running the member.
        /// </summary>
        public MemberVersion Version { get; }

        /// <summary>
        /// Determines whether the member is a "lite" member.
        /// </summary>
        /// <remarks>
        /// <para>Lite members do not own partitions.</para>
        /// </remarks>
        public bool IsLiteMember {get; }

        /// <summary>
        /// Gets the attributes of the member.
        /// </summary>
        public IReadOnlyDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets the address map.
        /// </summary>
        internal IReadOnlyDictionary<EndpointQualifier, NetworkAddress> AddressMap { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => Equals(obj as MemberInfo);

        /// <summary>
        /// Determines whether this <see cref="MemberInfo"/> instance is equal to another <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="other">The other <see cref="MemberInfo"/> instance.</param>
        /// <returns><c>true</c> if this <see cref="MemberInfo"/> instance and the other <see cref="MemberInfo"/> instance
        /// are considered being equal; otherwise <c>false</c>.</returns>
        public bool Equals(MemberInfo other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            // compare members on what matters: the id and the connect address

            return
                Id == other.Id &&
                ConnectAddress == other.ConnectAddress;
        }

        /// <summary>
        /// Determines whether two <see cref="MemberInfo"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="MemberInfo"/> instance.</param>
        /// <param name="right">The second <see cref="MemberInfo"/> instance.</param>
        /// <returns><c>true</c> if the two <see cref="MemberInfo"/> instances are considered being equal;
        /// otherwise <c>false</c>.</returns>
        public static bool operator ==(MemberInfo left, MemberInfo right)
            => left is null ? right is null : left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="MemberInfo"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="MemberInfo"/> instance.</param>
        /// <param name="right">The second <see cref="MemberInfo"/> instance.</param>
        /// <returns><c>true</c> if the two <see cref="MemberInfo"/> instances are considered being not equal;
        /// otherwise <c>false</c>.</returns>
        public static bool operator !=(MemberInfo left, MemberInfo right)
            => !(left == right);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Id, ConnectAddress);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"(Member Address = {Address}, PublicAddress = {PublicAddress}, ConnectAddress = {ConnectAddress}, Id = {Id}, IsLite = {IsLiteMember})";
        }

        public string ToShortString(bool flagConnectAddress)
            => $"Id={Id.ToShortString()} Internal={Address}{(!flagConnectAddress || UsePublicAddress ? "" : "*")} Public={(PublicAddress == null ? "<none>" : PublicAddress.ToString())}{(flagConnectAddress && UsePublicAddress ? "*" : "")}";
    }
}
