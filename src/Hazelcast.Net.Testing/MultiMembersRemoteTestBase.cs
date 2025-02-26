// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests that require a remote environment with multiple members.
    /// </summary>
    public class MultiMembersRemoteTestBase : ClusterRemoteTestBase
    {
        [OneTimeTearDown]
        public async Task MembersOneTimeTearDown()
        {
            // terminate & remove members
            foreach (var member in RcMembers.Values)
            {
                await RemoveMember(member.Uuid);
            }
        }

        /// <summary>
        /// Adds a member to the cluster.
        /// </summary>
        /// <returns>The added member.</returns>
        protected async Task<Member> AddMember()
        {
            var member = await RcClient.StartMemberAsync(RcCluster);
            RcMembers[Guid.Parse(member.Uuid)] = member;
            return member;
        }

        /// <summary>
        /// Removes a member from the cluster.
        /// </summary>
        /// <param name="memberId">The identifier of the member to remove.</param>
        /// <returns>A task that will complete when the member has been removed.</returns>
        protected async Task RemoveMember(Guid memberId)
        {
            if (RcMembers.TryRemove(memberId, out var member))
            {
                await RcClient.StopMemberAsync(RcCluster, member);
            }
        }

        /// <summary>
        /// Removes a member from the cluster.
        /// </summary>
        /// <param name="memberId">The identifier of the member to remove.</param>
        /// <returns>A task that will complete when the member has been removed.</returns>
        protected async Task RemoveMember(string memberId)
        {
            await RemoveMember(Guid.Parse(memberId));
        }

        /// <summary>
        /// Gets the remote members.
        /// </summary>
        protected ConcurrentDictionary<Guid, Member> RcMembers { get; } = new ConcurrentDictionary<Guid, Member>();
    }
}
