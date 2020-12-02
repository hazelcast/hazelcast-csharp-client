// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.ObjectModel;
using Hazelcast.Networking;

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents a member of a cluster.
    /// </summary>
    public class MemberInfo : IEquatable<MemberInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfo"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the member.</param>
        /// <param name="address">The network address of the member.</param>
        /// <param name="version">The version of the server running the member.</param>
        /// <param name="isLiteMember">Whether the member is a "lite" member.</param>
        /// <param name="attributes">Attributes of the member.</param>
        public MemberInfo(Guid id, NetworkAddress address, MemberVersion version, bool isLiteMember, IDictionary<string, string> attributes)
        {
            Id = id;
            Address = address;
            Version = version;
            IsLiteMember = isLiteMember;
            Attributes = new ReadOnlyDictionary<string, string>(attributes);
            AddressMap = new Dictionary<EndpointQualifier, NetworkAddress>();
        }

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
        internal MemberInfo(NetworkAddress address, Guid id, IDictionary<string, string> attributes, bool isLiteMember, MemberVersion version, bool addressMapExists, IDictionary<EndpointQualifier, NetworkAddress> addressMap)
            : this(id, address, version, isLiteMember, attributes)
        {
            // yes, this constructor could be simplified, but at the moment
            // it is what codecs expect, so don't simplify it!

            if (addressMapExists) AddressMap = addressMap;
        }

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
        internal IDictionary<EndpointQualifier, NetworkAddress> AddressMap { get; }

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

            return
                Id == other.Id;
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
        public override int GetHashCode() => Id.GetHashCode();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Member [{Address.Host}]:{Address.Port} - {Id}{(IsLiteMember ? " lite" : "")}";
        }
    }
}
