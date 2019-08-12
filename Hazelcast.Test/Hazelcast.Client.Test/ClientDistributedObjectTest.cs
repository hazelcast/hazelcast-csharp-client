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
using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class ClientDistributedObjectTest : SingleMemberBaseTest
    {
        [TearDown]
        public void TearDown()
        {
            foreach (var distributedObject in Client.GetDistributedObjects())
            {
                distributedObject.Destroy();
            }
        }

        [Test]
        public void TestDistributedObjectListener()
        {
            var createdLatch = new CountdownEvent(1);
            var destroyedLatch = new CountdownEvent(1);
            var regId = Client.AddDistributedObjectListener(
                new DistributedObjectListener(createdLatch, destroyedLatch));
            Assert.IsNotNull(regId, "registrationId");

            var name = TestSupport.RandomString();
            var topic = Client.GetTopic<object>(name);
            TestSupport.AssertOpenEventually(createdLatch, 10);
            topic.Destroy();
            TestSupport.AssertOpenEventually(destroyedLatch, 10);

            Assert.IsTrue(Client.RemoveDistributedObjectListener(regId),
                "Client.RemoveDistributedObjectListener(regId)");
        }

        [Test]
        public void TestDistributedObjectListener_validateEventContent()
        {
            var listener = new DistributedObjectListenerWithAssert();
            var regId = Client.AddDistributedObjectListener(listener);
            Assert.IsNotNull(regId, "registrationId");
            var client2 = CreateClient();
            client2.GetMap<string, int>(TestSupport.RandomString());

            var distributedObjectEvent = listener.Events.Take();

            Assert.NotNull(distributedObjectEvent);
            Assert.True(distributedObjectEvent.GetDistributedObject() is IMap<object, object>);

            var distributedObject = distributedObjectEvent.GetDistributedObject<IMap<string, int>>();
            Assert.NotNull(distributedObject);
            Assert.NotNull(distributedObjectEvent.GetObjectName());
            Assert.AreEqual(ServiceNames.Map, distributedObjectEvent.GetServiceName());
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
            var mapName = TestSupport.RandomString();
            var map = Client.GetMap<int, int>(mapName);
            Client.GetTopic<int>(TestSupport.RandomString());
            Client.GetSemaphore(TestSupport.RandomString());

            map.Put(1, 1);
            var client2 = CreateClient();

            Assert.AreEqual(3, client2.GetDistributedObjects().Count);

            var map2 = client2.GetMap<int, int>(mapName);
            Assert.AreEqual(1, map2.Get(1));
        }

        [Test]
        public void TestProxyManager_getOrCreate_Assignable()
        {
            var distributedObject = Client.GetDistributedObject<IDistributedObject>(ServiceNames.Map, "testNameId");
            var map = Client.GetDistributedObject<IMap<string, long>>(ServiceNames.Map, "testNameId");
            Assert.NotNull(distributedObject);
            Assert.NotNull(map);
        }

        [Test]
        public void TestProxyManager_getOrCreate_NotAssignable()
        {
            var distributedObject = Client.GetDistributedObject<IMap<string, string>>(ServiceNames.Map, "testNameId");
            Assert.NotNull(distributedObject);
            try
            {
                Client.GetDistributedObject<IMap<string, long>>(ServiceNames.Map, "testNameId");
            }
            catch (InvalidCastException e)
            {
                Assert.IsNotEmpty(e.Message);
            }
        }

        class DistributedObjectListener : IDistributedObjectListener
        {
            readonly CountdownEvent _createdLatch;
            readonly CountdownEvent _destroyedLatch;

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

        class DistributedObjectListenerWithAssert : IDistributedObjectListener
        {
            public BlockingCollection<DistributedObjectEvent> Events { get; } = new BlockingCollection<DistributedObjectEvent>();

            public void DistributedObjectCreated(DistributedObjectEvent distributedObjectEvent)
            {
                Events.Add(distributedObjectEvent);
            }

            public void DistributedObjectDestroyed(DistributedObjectEvent @event)
            {
            }
        }
    }
}