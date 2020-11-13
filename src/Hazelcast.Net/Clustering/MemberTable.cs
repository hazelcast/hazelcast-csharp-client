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
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberTable"/> class.
        /// </summary>
        public MemberTable()
            : this(0, new Dictionary<Guid, MemberInfo>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberTable"/> class.
        /// </summary>
        /// <param name="version">The version of the member table.</param>
        /// <param name="members">The members.</param>
        public MemberTable(int version, Dictionary<Guid, MemberInfo> members)
        {
            Version = version;
            Members = members;
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
        public int Count => Members.Count;

        /// <summary>
        /// Gets the members.
        /// </summary>
        public Dictionary<Guid, MemberInfo> Members { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append("MemberTable (");
            text.Append(Count);
            text.Append(" members: ");
            text.Append(string.Join(", ", Members.Values));
            text.Append(')');
            return text.ToString();
        }
    }
}
