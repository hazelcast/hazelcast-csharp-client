using System.Threading;
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