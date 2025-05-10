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
using System.Threading.Tasks;
using Hazelcast.Exceptions;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a cluster-wide unique identifier generator. The identifiers are <see cref="long"/> primitive values
    /// in the range from <c>0</c> to <see cref="long.MaxValue"/>, and are k-ordered (roughly ordered).
    /// </summary>
    /// <remarks>
    /// <para>The identifiers contain a timestamp component, and a member identifier component which is assigned
    /// when the member joins the cluster. This allows identifiers to be ordered and unique without any coordination
    /// between members, thus making the generator safe even in split-brain scenario.</para>
    /// <para>The timestamp component is composed of 41 bits representing milliseconds since Jan. 1st, 2018 00:00UTC.
    /// This caps the useful lifespan of the generator to little less that 70 years, i.e. until ~2088.</para>
    /// <para>The sequence component is composed of 6 bits. If more than 64 identifiers are requested in a single
    /// milliseconds, identifiers will gracefully overflow to the next milliseconds while still guaranteeing uniqueness.</para>
    /// <para>The member-side implementation does not allow overflowing by more than 15 seconds, and if identifiers are
    /// requested at a higher rate, calls will block. Note that however clients are able to generate identifiers faster,
    /// because each call goes to a different (random) member and the 64 identifiers/ms limit is for one single member.</para>
    /// <para>It is possible to generate identifiers on any member or client as lon as there is at least one member with
    /// join version smaller than 2^16 in the cluster. The remedy is to restart the cluster, and then node identifiers
    /// will be assigned from zero again. Uniqueness after a restart is guaranteed by the timestamp component.</para>

    /// Timestamp component is in milliseconds since 1.1.2018, 0:00 UTC and has 41 bits.
    /// This caps the useful lifespan of the generator to little less than 70 years (until ~2088).
    /// The sequence component is 6 bits. If more than 64 IDs are requested in single millisecond,
    /// IDs will gracefully overflow to the next millisecond and uniqueness is guaranteed in this case.
    /// The implementation does not allow overflowing by more than 15 seconds,
    /// if IDs are requested at higher rate, the call will block.
    /// Note, however, that clients are able to generate even faster because each call goes to a different (random) member,
    /// and the 64 IDs/ms limit is for single member.
    /// </para>
    /// <para>
    /// It is possible to generate IDs on any member or client as long as there is at least one member with join version smaller than 2^16 in the cluster.
    /// The remedy is to restart the cluster: nodeId will be assigned from zero again.
    /// Uniqueness after the restart will be preserved thanks to the timestamp component.
    /// </para>
    /// </remarks>
    public interface IFlakeIdGenerator: IDistributedObject
    {
        /// <summary>
        /// Gets a new cluster-wide unique identifier.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method goes to a random member and gets a batch of IDs, which will then be returned locally for limited time.
        /// The pre-fetch size and the validity time can be configured via <see cref="FlakeIdGeneratorOptions"/>.
        /// </para>
        /// <para>Values returned from this method may not be strictly ordered.</para>
        /// </remarks>
        /// <returns>A <see cref="long"/> value representing a cluster-wide unique identifier.</returns>
        ///
        /// <exception cref="HazelcastException">
        /// If node ID for all members in the cluster is out of valid range.
        /// </exception>
        ValueTask<long> GetNewIdAsync();
    }
}
