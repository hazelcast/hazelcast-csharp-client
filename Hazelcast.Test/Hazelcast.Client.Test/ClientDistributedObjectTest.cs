// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    internal class ClientDistributedObjectTest : SingleMemberBaseTest
    {
        [Test]
        public void TestDistributedObjectListener()
        {
            var createdLatch = new CountdownEvent(1);
            var destroyedLatch = new CountdownEvent(1);
            var regId = Client.AddDistributedObjectListener(new DistributedObjectListener(createdLatch, destroyedLatch));
            Assert.IsNotNull(regId, "regisrationId");

            var name = TestSupport.RandomString();
            var topic = Client.GetTopic<object>(name);
            TestSupport.AssertOpenEventually(createdLatch, 10);
            topic.Destroy();
            TestSupport.AssertOpenEventually(destroyedLatch, 10);

            Assert.IsTrue(Client.RemoveDistributedObjectListener(regId), "Client.RemoveDistributedObjectListener(regId)");
        }

        [Test]
        public void TestGetDistributedObjects()
        {
            var map = Client.GetMap<int, int>(TestSupport.RandomString());
            var topic = Client.GetTopic<int>(TestSupport.RandomString());
            var semaphore = Client.GetSemaphore(TestSupport.RandomString());

            
            Assert.AreEqual(3, Client.GetDistributedObjects().Count);

            map.Destroy();
            topic.Destroy();
            semaphore.Destroy();

            Assert.AreEqual(0, Client.GetDistributedObjects().Count);
        }

        [Test]
        public void TestGetDistributedObjectsFromAnotherClient()
        {
            String mapName = TestSupport.RandomString();
            var map = Client.GetMap<int, int>(mapName);
            Client.GetTopic<int>(TestSupport.RandomString());
            Client.GetSemaphore(TestSupport.RandomString());

            map.Put(1, 1);
            var client2 = CreateClient();

            Assert.AreEqual(3, client2.GetDistributedObjects().Count);

            var map2 = client2.GetMap<int, int>(mapName);
            Assert.AreEqual(1, map2.Get(1));
        }

        private class DistributedObjectListener : IDistributedObjectListener
        {
            private readonly CountdownEvent _createdLatch;
            private readonly CountdownEvent _destroyedLatch;

            public DistributedObjectListener(CountdownEvent createdLatch, CountdownEvent destroyedLatch)
            {
                _createdLatch = createdLatch;
                _destroyedLatch = destroyedLatch;
            }

            public void DistributedObjectCreated(DistributedObjectEvent @event)
            {
                _createdLatch.Signal();
            }

            public void DistributedObjectDestroyed(DistributedObjectEvent @event)
            {
                _destroyedLatch.Signal();
            }
        }
    }
}