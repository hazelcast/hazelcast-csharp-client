// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents the routing modes from client to cluster.
    /// </summary>
    public enum RoutingModes : byte
    {
        /// <summary>
        /// Connects to a single member which is given on the options.
        /// </summary>
        SingleMember = 0,

        /// <summary>
        /// Connects to all members in the cluster. The client will receive
        /// member list updates and connect to all members.
        /// <para>Client will route the key based operations to owner of
        /// the key at the best effort.</para>
        /// <para>Note that it however does not guarantee that the operation will always be
        /// executed on the owner, as the member table is only updated every 10 seconds.</para>
        /// </summary>
        AllMembers = 1,

        /// <summary>
        /// Connects only to selected group of members in the cluster based on selected grouping strategy.
        /// </summary>
        MultiMember = 2,
    }

    /// <summary>
    /// Routing strategy for selected routing modes.
    /// </summary>
    public enum RoutingStrategy
    {
        /// <summary>
        /// Clients connects to subset of the cluster based on partition groups.
        /// <para>This strategy only valid when RoutingMode is <see cref="RoutingModes.MultiMember"/></para>
        /// </summary>
        PartitionGroups
    }
}
