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
using System.Text;
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a cluster member table.
    /// </summary>
    internal class MemberTable
    {
        private readonly Dictionary<Guid, MemberInfo> _members;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberTable"/> class.
        /// </summary>
        public MemberTable()
            : this(-1, new Dictionary<Guid, MemberInfo>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberTable"/> class.
        /// </summary>
        /// <param name="version">The version of the member table.</param>
        /// <param name="members">The members.</param>
        public MemberTable(int version, Dictionary<Guid, MemberInfo> members)
        {
            Version = version;
            _members = members;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberTable"/> class.
        /// </summary>
        /// <param name="version">The version of the member table.</param>
        /// <param name="members">The members.</param>
        public MemberTable(int version, IEnumerable<MemberInfo> members)
            : this(version, members.ToDictionary(x => x.Id, x => x))
        {}

        /// <summary>
        /// Gets the version of the member table.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Gets the number of members in the table.
        /// </summary>
        public int Count => _members.Count;

        /// <summary>
        /// Gets the members.
        /// </summary>
        public IReadOnlyCollection<MemberInfo> Members => _members.Values;

        /// <summary>
        /// Determines whether the table contains a member.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member.</param>
        /// <returns><c>true</c> if the table contains the member; otherwise <c>false</c>.</returns>
        public bool ContainsMember(Guid memberId) => _members.ContainsKey(memberId);

        /// <summary>
        /// Tries to get the member associated with the specified identifier.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member.</param>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if the table contains the member; otherwise <c>false</c>.</returns>
        public bool TryGetMember(Guid memberId, out MemberInfo member) => _members.TryGetValue(memberId, out member);

        /// <summary>
        /// Finds a larger same-version group of data members from a collection of members.
        /// Otherwise returns a random member from the group. If the same-version
        /// groups have the same size, returns a member from the newer group.
        /// </summary>
        /// <returns><see cref="MemberInfo"/> if one is found or <c>null</c> otherwise.</returns>
        /// <exception cref="InvalidOperationException">If there are more than 2 distinct member versions found.</exception>
        public MemberInfo FindMemberOfLargerSameVersionGroup()
        {
            (MemberVersion version0, MemberVersion version1) = (null, null);
            var (count0, count1) = (0, 0);

            foreach (var member in Members)
            {
                if (member.IsLiteMember)
                    continue;

                var memberVersion = member.Version;

                if (version0 == null || version0.Equals(memberVersion, ignorePatchVersion: true))
                {
                    version0 = memberVersion;
                    count0++;
                }
                else if (version1 == null || version1.Equals(memberVersion, ignorePatchVersion: true))
                {
                    version1 = memberVersion;
                    count1++;
                }
                else
                {
                    var strVersion0 = version0.ToString(ignorePatchVersion: true);
                    var strVersion1 = version1.ToString(ignorePatchVersion: true);
                    var strVersion = memberVersion.ToString(ignorePatchVersion: true);

                    throw new InvalidOperationException(
                        $"More than 2 distinct member versions found: {strVersion0}, {strVersion1}, {strVersion}"
                    );
                }
            }

            // no data members
            if (count0 == 0)
                return null;

            int count;
            MemberVersion version;

            if (count0 > count1 || (count0 == count1 && version0 > version1))
                (count, version) = (count0, version0);
            else
                (count, version) = (count1, version1);

            // otherwise return a random member from the larger group
            var randomIndex = RandomProvider.Next(count);
            foreach (var member in Members)
            {
                if (!member.IsLiteMember && member.Version.Equals(version, ignorePatchVersion: true))
                {
                    randomIndex--;
                    if (randomIndex < 0)
                        return member;
                }
            }

            throw new Exception($"Reached unexpected state in {nameof(FindMemberOfLargerSameVersionGroup)}.");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append("MemberTable (");
            text.Append(Count);
            text.Append(" members: ");
            text.Append(string.Join(", ", Members));
            text.Append(')');
            return text.ToString();
        }
    }
}
