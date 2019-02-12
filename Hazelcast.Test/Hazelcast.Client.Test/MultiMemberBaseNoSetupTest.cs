// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;
using Hazelcast.Remote;
using Hazelcast.Config;
using System.Collections.Generic;

namespace Hazelcast.Client.Test
{
    public class MultiMemberBaseNoSetupTest : HazelcastTestSupport
    {
        protected IHazelcastInstance Client { get; private set; }
        protected HazelcastClient ClientInternal { get; private set; }
        protected ThreadSafeRemoteController RemoteController { get; private set; }
        protected Cluster HzCluster { get; private set; }
        protected readonly List<Remote.Member> MemberList = new List<Remote.Member>();

        public virtual void SetupCluster()
        {
            RemoteController = (ThreadSafeRemoteController)CreateRemoteController();
            HzCluster = CreateCluster(RemoteController, GetServerConfig());
            InitMembers();
            Client = CreateClient();
            ClientInternal = ((HazelcastClientProxy)Client).GetClient();
        }

        public virtual void ShutdownRemoteController()
        {
            HazelcastClient.ShutdownAll();
            StopRemoteController(RemoteController);
        }

        protected virtual void InitMembers()
        {
            MemberList.Add(RemoteController.startMember(HzCluster.Id));
        }

        protected virtual string GetServerConfig()
        {
            return Hazelcast.Test.Resources.hazelcast;
        }

        protected override void ConfigureGroup(ClientConfig config)
        {
            config.GetGroupConfig().SetName(HzCluster.Id).SetPassword(HzCluster.Id);
        }
    }
}
