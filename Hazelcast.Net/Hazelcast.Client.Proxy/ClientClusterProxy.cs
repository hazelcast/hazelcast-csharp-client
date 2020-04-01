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
using Hazelcast.Client.Spi;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientClusterProxy : ICluster
    {
        private readonly IClientClusterService _clusterService;

        public ClientClusterProxy(IClientClusterService clusterService)
        {
            _clusterService = clusterService;
        }

        public virtual string AddMembershipListener(IMembershipListener listener)
        {
            return _clusterService.AddMembershipListener(listener);
        }

        public virtual bool RemoveMembershipListener(string registrationId)
        {
            return _clusterService.RemoveMembershipListener(registrationId);
        }

        public virtual IList<IMember> GetMembers()
        {
            var members = _clusterService.GetMemberList();

            return members != null ? new List<IMember>(members) : new List<IMember>();
        }

        public virtual IMember GetLocalMember()
        {
            throw new NotSupportedException("IClient has no local member!");
        }

        public virtual long GetClusterTime()
        {
            return _clusterService.GetClusterTime();
        }
    }
}