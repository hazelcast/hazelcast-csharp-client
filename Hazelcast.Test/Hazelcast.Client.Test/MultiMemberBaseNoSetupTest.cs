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
using System.Collections.Concurrent;
using Hazelcast.Core;
using Hazelcast.Remote;
using Hazelcast.Config;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Logging;
using Member = Hazelcast.Remote.Member;

namespace Hazelcast.Client.Test
{
    public class MultiMemberBaseNoSetupTest : HazelcastTestSupport
    {
        protected IHazelcastInstance Client { get; private set; }
        protected HazelcastClient ClientInternal { get; private set; }
        protected IRemoteController RemoteController { get; private set; }
        protected Cluster HzCluster { get; private set; }
        private readonly ConcurrentDictionary<Guid, Remote.Member> MemberList = new ConcurrentDictionary<Guid, Remote.Member>();

        public virtual void SetupCluster(Action initMembers)
        {
            RemoteController = CreateRemoteController();
            HzCluster = CreateCluster(RemoteController, GetServerConfig());
            initMembers();
            Client = CreateClient();
            ClientInternal = (HazelcastClient)Client;
        }
        public virtual void SetupCluster()
        {
            SetupCluster(InitMembers);
        }

        public virtual void ShutdownRemoteController()
        {
            HazelcastClient.ShutdownAll();
            StopRemoteController(RemoteController);
            MemberList.Clear();
        }

        protected virtual void InitMembers()
        {
            StartNewMember();
        }

        protected virtual string GetServerConfig()
        {
            return Hazelcast.Test.Resources.Hazelcast;
        }

        protected override void ConfigureGroup(Configuration config)
        {
            config.ClusterName = HzCluster.Id;
        }

        protected Guid StartNewMember()
        {
            var newMember = RemoteController.startMember(HzCluster.Id);
            var guid = Guid.Parse(newMember.Uuid);
            MemberList.TryAdd(guid,newMember);
            return guid;
        }
        
        protected void ShutdownMember(Guid memberUuid)
        {
            if(MemberList.TryRemove(memberUuid, out var member))
            {
                StopMember(RemoteController, HzCluster, member);
            }
        }
        protected bool ShutdownCluster()
        {
            _logger.Info($"Shutting cluster {HzCluster.Id}");
            var result = StopCluster(RemoteController, HzCluster);
            if (result)
            {
                MemberList.Clear();
            }
            return result;
        }
    }
}
