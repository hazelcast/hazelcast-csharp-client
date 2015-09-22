using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class NonSmartRoutingTest : HazelcastBaseTest
    {
        private int _nodeId;

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetNetworkConfig().SetSmartRouting(false);
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            _nodeId = AddNodeAndWait();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            RemoveNodeAndWait(_nodeId);
        }

        [Test]
        public void TestPutAllWithNonSmartRouting()
        {
            var map = Client.GetMap<string, string>(Name);
            int n = 1000;
            Dictionary<string, string> toInsert = new Dictionary<string, string>();
            for (int i = 0; i < n; i++)
            {
                toInsert.Add(TestSupport.RandomString(), TestSupport.RandomString());
            }
            map.PutAll(toInsert);

            var resp = map.GetAll(toInsert.Keys);

            Assert.AreEqual(toInsert, resp);
        }

        [Test]
        public void TestListenerWithNonSmartRouting()
        {
            var map = Client.GetMap<string, string>(Name);
            int n = 4;

            var keys = TestSupport.RandomArray(TestSupport.RandomString, 10);
            var registrations = new List<String>();
            var tasks = new List<Task>();
            foreach (var key in keys)
            {
                var tcs = new TaskCompletionSource<bool>();
                var id = map.AddEntryListener(new EntryListener<string, string>
                {
                    EntryAddedAction = e => tcs.SetResult(true)
                }, key, false);
                registrations.Add(id);
                tasks.Add(tcs.Task);
            }

            foreach (var key in keys)
            {
                map.Put(key, TestSupport.RandomString());
            }

            Assert.IsTrue(Task.WaitAll(tasks.ToArray(), 500), "Did not get all entry added events within 500ms");
            foreach (var id in registrations)
            {
                Assert.IsTrue(map.RemoveEntryListener(id));
            }
        }
    }
}
