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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    class ClientReconnectionTest : HazelcastBaseTest
    {
        [Test]
        public void TestListenerReconnect()
        {
            var name = TestSupport.RandomString();
            var key = "key";
            var map = Client.GetMap<string, string>(name);
            int eventCount = 0;
            var count = 2;
            var regId = map.AddEntryListener(new EntryListener<string, string>
            {
                EntryAddedAction = e =>
                {
                    Interlocked.Increment(ref eventCount);
                }
            }, true);

            // try to start and stop the instance several times
            for (int i = 0; i < count; i++)
            {
                var clientDisconnected = TestSupport.WaitForClientState(Client, LifecycleEvent.LifecycleState.ClientDisconnected);
                Cluster.RemoveNode();
                TestSupport.AssertCompletedEventually(clientDisconnected, taskName: "clientDisconnected");
                Interlocked.Exchange(ref eventCount, 0);
                var clientConnected = TestSupport.WaitForClientState(Client, LifecycleEvent.LifecycleState.ClientConnected);
                Cluster.AddNode();
                TestSupport.AssertCompletedEventually(clientConnected, taskName: "clientConnected");

                TestSupport.AssertTrueEventually(() =>
                {
                    map.Put(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                    return eventCount > 0;
                });
            }
            Assert.IsTrue(map.RemoveEntryListener(regId));
            map.Destroy();
        }

        [Test]
        public void TestReconnect()
        {
            var name = TestSupport.RandomString();
            var map = Client.GetMap<string, string>(name);
            map.Put("key", "value");
            Assert.AreEqual("value", map.Get("key"));

            var clientDisconnected = TestSupport.WaitForClientState(Client, LifecycleEvent.LifecycleState.ClientDisconnected);
            Cluster.RemoveNode();
            TestSupport.AssertCompletedEventually(clientDisconnected, taskName: "clientDisconnected");

            var clientConnected = TestSupport.WaitForClientState(Client, LifecycleEvent.LifecycleState.ClientConnected);
            Cluster.AddNode();
            TestSupport.AssertCompletedEventually(clientConnected, taskName: "clientConnected");

            map.Put("key", "value2");
            Assert.AreEqual("value2", map.Get("key"));

            map.Destroy();
        }
    }
}
