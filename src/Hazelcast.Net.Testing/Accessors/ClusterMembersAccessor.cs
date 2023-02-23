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
using Hazelcast.Clustering;

namespace Hazelcast.Testing.Accessors
{
    internal class ClusterMembersAccessor: AccessorBase<ClusterMembers>
    {
        internal ClusterMembersAccessor(ClusterMembers clusterMembers): base(clusterMembers)
        { }

        internal MemberTable Members
        {
            get => GetField<MemberTable>("_members");
            set => SetField("_members", value);
        }

        internal Dictionary<Guid, MemberConnection> Connections
        {
            get => GetField<Dictionary<Guid, MemberConnection>>("_connections");
            set => SetField("_connections", value);
        }
    }
}