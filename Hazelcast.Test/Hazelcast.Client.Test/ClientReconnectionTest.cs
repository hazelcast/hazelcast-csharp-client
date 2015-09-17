using System;
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
            var name = Name;
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
            var name = Name;
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
