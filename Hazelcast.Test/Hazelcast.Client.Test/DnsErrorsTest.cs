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
using System.Net;
using System.Net.Sockets;
using Hazelcast.Config;
using Hazelcast.Remote;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class DnsErrorsTest : HazelcastTestSupport
    {
        private RemoteController.Client _remoteController;
        private Cluster _cluster;

        [SetUp]
        public void Setup()
        {
            _remoteController = CreateRemoteController();
            _cluster = CreateCluster(_remoteController);

            StartMember(_remoteController, _cluster);
        }

        [Test]
        public void SingleFailureAtAddressResolutionShouldNotBlowUpClient()
        {
            using (ThrowGetHostAddressesAt(2))
            {
                var client = CreateClient();

                client.Shutdown();
            }
        }

        [TearDown]
        public void TearDown()
        {
            HazelcastClient.ShutdownAll();
            StopCluster(_remoteController, _cluster);
            StopRemoteController(_remoteController);
        }

        protected override void ConfigureGroup(ClientConfig config)
        {
            config.GetGroupConfig().SetName(_cluster.Id).SetPassword(_cluster.Id);
        }

        static IDisposable ThrowGetHostAddressesAt(int failAt)
        {
            var count = 0;
            Func<string, IPAddress[]> over =
                s =>
                {
                    count++;
                    if (count == failAt)
                    {
                        throw new SocketException((int)SocketError.HostNotFound);
                    }

                    return Dns.GetHostAddresses(s);
                };

            return Overrides.Dns.GetHostAddresses(over);
        }
    }
}