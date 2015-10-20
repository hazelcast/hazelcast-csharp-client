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

﻿using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    internal class ClientDistributedObjectTest : HazelcastBaseTest
    {
        [Test]
        public void TestGetDistributedObjects()
        {
            var queue = Client.GetMap<int,int>(TestSupport.RandomString());
            var topic = Client.GetTopic<int>(TestSupport.RandomString());
            var semaphore = Client.GetSemaphore(TestSupport.RandomString());

            Assert.AreEqual(3, Client.GetDistributedObjects().Count);

            queue.Destroy();
            topic.Destroy();
            semaphore.Destroy();

            Assert.AreEqual(0, Client.GetDistributedObjects().Count);
        }


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