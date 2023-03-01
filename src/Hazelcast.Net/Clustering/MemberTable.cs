// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
