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

#nullable enable

using System;

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents the state of a member of a cluster.
    /// </summary>
    public readonly struct MemberInfoState : IEquatable<MemberInfoState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfoState"/> struct.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="isConnected">Whether the member is connected.</param>
        internal MemberInfoState(MemberInfo member, bool isConnected)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            IsConnected = isConnected;
        }

        /// <summary>
        /// Gets the member.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Whether the member is connected.
        /// </summary>
        public bool IsConnected { get; }

        public override bool Equals(object? obj)
            => obj is MemberInfoState state && Equals(state);

        public override int GetHashCode()
            => Member.GetHashCode();

        public static bool operator ==(MemberInfoState left, MemberInfoState right)
            => left.Member.Equals(right.Member); // .Member cannot be null

        public static bool operator !=(MemberInfoState left, MemberInfoState right)
            => !(left == right);

        public bool Equals(MemberInfoState other)
            => Member.Equals(other.Member);
    }
}
