/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    internal abstract class AbstractLoadBalancer : ILoadBalancer, IMembershipListener
    {
        private volatile ICluster _clusterRef;
        private IMember[] _members;

        public IMember[] Members
        {
            get { return _members; }
            private set { Interlocked.Exchange(ref _members, value); }
        }

        public void MemberAdded(MembershipEvent membershipEvent)
        {
            SetMembersFromCluster();
        }

        public void MemberRemoved(MembershipEvent membershipEvent)
        {
            SetMembersFromCluster();
        }

        public void MemberAttributeChanged(MemberAttributeEvent memberAttributeEvent){}

        public void Init(ICluster cluster, ClientConfig config)
        {
            _clusterRef = cluster;
            SetMembersFromCluster();
            cluster.AddMembershipListener(this);
        }

        public abstract IMember Next();

        protected internal void SetMembersFromCluster()
        {
            ICluster cluster = _clusterRef;
            if (cluster != null)
            {
                ISet<IMember> memberSet = cluster.GetMembers();
                Members = memberSet.ToArray();
            }
        }
    }
}