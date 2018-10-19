// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Remote;
using Hazelcast.Test;
using Hazelcast.Util;
using NUnit.Framework;
using Thrift.Protocol;
using Thrift.Transport;
using Member = Hazelcast.Remote.Member;

namespace Hazelcast.Client.Test
{
    public class HazelcastTestSupport
    {
        private readonly ILogger _logger;

        private readonly ConcurrentQueue<UnobservedTaskExceptionEventArgs> _unobservedExceptions = new ConcurrentQueue<UnobservedTaskExceptionEventArgs>();

        public HazelcastTestSupport()
        {
#if DEBUG
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "trace");

#else
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");
#endif
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "finest");
            _logger = Logger.GetLogger(GetType().Name);

            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Warning("UnobservedTaskException Error sender:" + sender);
            _logger.Warning("UnobservedTaskException Error.", e.Exception);
            _unobservedExceptions.Enqueue(e);
        }

        [TearDown]
        public void BaseTearDown()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (var exceptionEventArg in _unobservedExceptions)
            {
                Assert.Fail("UnobservedTaskException occured. {0}", exceptionEventArg.Exception.Flatten().InnerException.StackTrace);
            }
        }

        [OneTimeTearDown]
        public void ShutdownAllClients()
        {
            HazelcastClient.ShutdownAll();
        }

        protected virtual void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().AddAddress("localhost:5701");
            config.GetNetworkConfig().SetConnectionAttemptLimit(20);
            config.GetNetworkConfig().SetConnectionAttemptPeriod(2000);
        }

        protected virtual void ConfigureGroup(ClientConfig config)
        {
        }

        protected virtual IHazelcastInstance CreateClient()
        {
            _logger.Info("Creating new client");
            var clientFactory = new HazelcastClientFactory();
            var resetEvent = new ManualResetEventSlim();
            var listener = new ListenerConfig(new LifecycleListener(l =>
            {
                if (l.GetState() == LifecycleEvent.LifecycleState.ClientConnected)
                {
                    resetEvent.Set();
                }
            }));
            var client = clientFactory.CreateClient(c =>
            {
                ConfigureClient(c);
                ConfigureGroup(c);
                //c.AddListenerConfig(listener);
            });
            //Assert.IsTrue(resetEvent.Wait(30*1000), "Client did not start after 30 seconds");
            return client;
        }

        protected virtual Cluster CreateCluster(RemoteController.Client remoteController)
        {
            _logger.Info("Creating cluster");
            var cluster = remoteController.createCluster(null, Resources.hazelcast);
            _logger.Info("Created cluster");
            return cluster;
        }

        protected virtual Cluster CreateCluster(RemoteController.Client remoteController, string xmlconfig)
        {
            _logger.Info("Creating cluster using custom config...");
            var cluster = remoteController.createCluster(null, xmlconfig);
            _logger.Info("Created cluster");
            return cluster;
        }

        protected RemoteController.Client CreateRemoteController()
        {
            
            TTransport transport = new TFramedTransport(new TSocket("localhost", 9701));
            transport.Open();
            TProtocol protocol = new TBinaryProtocol(transport);
            return new ThreadSafeRemoteController(protocol);
        }

        protected void StopRemoteController(RemoteController.Client client)
        {
            client.exit();
            client.InputProtocol.Transport.Close();
        }

        protected int GetUniquePartitionOwnerCount(IHazelcastInstance client)
        {
            var proxy = ((HazelcastClientProxy) client);
            var partitionService = proxy.GetClient().GetClientPartitionService();
            var count = partitionService.GetPartitionCount();
            var owners = new HashSet<Address>();
            for (var i = 0; i < count; i++)
            {
                owners.Add(partitionService.GetPartitionOwner(i));
            }
            return owners.Count;
        }

        protected virtual void ResumeMember(RemoteController.Client remoteController, Cluster cluster, Member member)
        {
            remoteController.resumeMember(cluster.Id, member.Uuid);
        }

        protected virtual Member StartMember(RemoteController.Client remoteController, Cluster cluster)
        {
            _logger.Info("Starting new member");
            return remoteController.startMember(cluster.Id);
        }

        protected Member StartMemberAndWait(IHazelcastInstance client, RemoteController.Client remoteController,
            Cluster cluster, int expectedSize)
        {
            var resetEvent = new ManualResetEventSlim();
            var regId = client.GetCluster().AddMembershipListener(new MembershipListener
            {
                OnMemberAdded = @event => resetEvent.Set()
            });
            var member = StartMember(remoteController, cluster);
            Assert.IsTrue(resetEvent.Wait(120*1000), "The member did not get added in 120 seconds");
            Assert.IsTrue(client.GetCluster().RemoveMembershipListener(regId));

            // make sure partitions are updated
            TestSupport.AssertTrueEventually(
                () => { Assert.AreEqual(expectedSize, GetUniquePartitionOwnerCount(client)); },
                60, "The partition list did not contain " + expectedSize + " partitions.");

            return member;
        }

        protected virtual void StopCluster(RemoteController.Client remoteController, Cluster cluster)
        {
            remoteController.shutdownCluster(cluster.Id);
        }

        protected virtual void StopMember(RemoteController.Client remoteController, Cluster cluster, Member member)
        {
            _logger.Info("Shutting down  member " + member.Uuid);
            remoteController.shutdownMember(cluster.Id, member.Uuid);
        }

        protected void StopMemberAndWait(IHazelcastInstance client, RemoteController.Client remoteController,
            Cluster cluster, Member member)
        {
            var resetEvent = new ManualResetEventSlim();
            var regId = client.GetCluster().AddMembershipListener(new MembershipListener
            {
                OnMemberRemoved = @event => resetEvent.Set()
            });
            StopMember(remoteController, cluster, member);
            Assert.IsTrue(resetEvent.Wait(120*1000), "The member did not get removed in 120 seconds");
            Assert.IsTrue(client.GetCluster().RemoveMembershipListener(regId));
        }

        protected virtual void SuspendMember(RemoteController.Client remoteController,  Cluster cluster, Member member)
        {
            remoteController.suspendMember(cluster.Id, member.Uuid);
        }

        protected object GenerateKeyForPartition(IHazelcastInstance client, int partitionId)
        {
            var partitionService = ((HazelcastClientProxy) client).GetClient().GetClientPartitionService();
            while (true) {
                var randomKey = TestSupport.RandomString();
                if (partitionService.GetPartitionId(randomKey) == partitionId) {
                    return randomKey;
                }
            }
        }
    }
}