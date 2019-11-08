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
        protected ThreadSafeRemoteController RemoteController { get; private set; }
        protected Cluster HzCluster { get; private set; }
        private readonly ConcurrentDictionary<string, Remote.Member> MemberList = new ConcurrentDictionary<string, Remote.Member>();

        public virtual void SetupCluster(Action initMembers)
        {
            RemoteController = (ThreadSafeRemoteController)CreateRemoteController();
            HzCluster = CreateCluster(RemoteController, GetServerConfig());
            initMembers();
            Client = CreateClient();
            ClientInternal = ((HazelcastClientProxy)Client).GetClient();
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
            return Hazelcast.Test.Resources.hazelcast;
        }

        protected override void ConfigureGroup(ClientConfig config)
        {
            config.SetClusterName(HzCluster.Id).SetClusterPassword(HzCluster.Id);
        }

        protected string StartNewMember()
        {
            var newMember = RemoteController.startMember(HzCluster.Id);
            MemberList.TryAdd(newMember.Uuid,newMember);
            return newMember.Uuid;
        }
        
        protected void ShutdownMember(string memberUuid)
        {
            Member member;
            if(MemberList.TryRemove(memberUuid, out member))
            {
                StopMember(RemoteController, HzCluster, member);
            }
        }
        protected bool ShutdownCluster()
        {
            _logger.Info(string.Format("Shutting cluster {0}", HzCluster.Id));
            var result = StopCluster(RemoteController, HzCluster);
            if (result)
            {
                MemberList.Clear();
            }
            return result;
        }
    }
}
