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

using System.Linq;
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    public abstract class AbstractLoadBalancer : ILoadBalancer, IInitialMembershipListener
    {
        private ICluster _cluster;
        private IMember[] _members;

        public IMember[] Members
        {
            get => _members;
            private set => Interlocked.Exchange(ref _members, value);
        }

        public void Init(ICluster cluster, ClientConfig config)
        {
            Volatile.Write(ref _cluster, cluster);
            SetMembersFromCluster();
            cluster.AddMembershipListener(this);
        }

        public abstract IMember Next();

        public void Init(InitialMembershipEvent membershipEvent)
        {
            SetMembersFromCluster();
        }

        public void MemberAdded(MembershipEvent membershipEvent)
        {
            SetMembersFromCluster();
        }

        public void MemberRemoved(MembershipEvent membershipEvent)
        {
            SetMembersFromCluster();
        }

        protected void SetMembersFromCluster()
        {
            Members = _cluster?.Members?.ToArray();
        }
    }
}