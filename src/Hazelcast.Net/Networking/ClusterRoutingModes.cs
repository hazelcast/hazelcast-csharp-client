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
namespace Hazelcast.Networking
{
    /// <summary>
    /// <p>Clients can connect to cluster members in one of 3 modes:</p>
    /// <see cref="RoutingModes.SingleMember"/> Client only connects to a single member.
    ///  <see cref="RoutingModes.AllMembers"/> Client connects to all cluster members.
    ///  <see cref="RoutingModes.MultiMember"/> Client only connects to a subset of members based on <see cref="RoutingStrategy"/>.
    /// </summary>
    public class RoutingMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingMode"/> class.
        /// </summary>
        public RoutingMode()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingMode"/> class.
        /// </summary>
        /// <param name="smartRouting"></param>
        public RoutingMode(bool smartRouting)
        {
            Mode = smartRouting ? RoutingModes.AllMembers : RoutingModes.SingleMember;
        }

        internal RoutingMode(RoutingMode routingMode)
        {
            Mode = routingMode.Mode;
            Strategy = routingMode.Strategy;
        }

        /// <summary>
        /// Routing mode of the client.
        /// </summary>
        public RoutingModes Mode { get; set; } = RoutingModes.AllMembers;

        /// <summary>
        /// The strategy for selected routing mode. Default is <see cref="RoutingStrategy.PartitionGroups"/>
        /// </summary>
        public RoutingStrategy Strategy { get; set; } = RoutingStrategy.PartitionGroups;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal RoutingMode Clone() => new(this);
    }
}
