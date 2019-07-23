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
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Test;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientHeartBeatTestForListener : MultiMemberBaseTest
    {
        protected override string GetServerConfig()
        {
            return Resources.hazelcast_hb;
        }

        [OneTimeTearDown]
        public void RestoreEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.interval", null);
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.interval", "1000");
        }

        [Test]
        public void TestClientShouldPingServerWhenClientOnlyListening()
        {
            var clientDisconnected = TestSupport.WaitForClientState(Client, LifecycleEvent.LifecycleState.ClientDisconnected);
            const int clientHeartbeatSeconds = 8;
            var publishMessageCount = 0;
            var listenedMessageCount = 0;
            var client2 = CreateClient();
            const string topicName = "topicName";
            var topic = Client.GetTopic<string>(topicName);
            var id = topic.AddMessageListener(msg => { listenedMessageCount++; });

            var topic2 = client2.GetTopic<string>(topicName);

            var begin = Clock.CurrentTimeMillis();
            while (Clock.CurrentTimeMillis() - begin < TimeSpan.FromSeconds(clientHeartbeatSeconds * 2).TotalMilliseconds)
            {
                topic2.Publish("message");
                publishMessageCount++;
            }

            topic.RemoveMessageListener(id);
            Assert.False(clientDisconnected.Wait(100), "Client should not be disconnected");
            Assert.AreEqual(publishMessageCount, listenedMessageCount);
        }
    }
}