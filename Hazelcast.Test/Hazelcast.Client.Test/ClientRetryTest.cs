using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientRetryTest : HazelcastBaseTest
    {
        private readonly int Count = 5000;

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetNetworkConfig().SetRedoOperation(true);
        }

        [Test]
        public void TestRetryRequestsWhenInstanceIsShutdown()
        {
            var nodeId = AddNodeAndWait();
            var map = Client.GetMap<int,string>(Name);
            for (int i = 0; i < Count; i++)
            {
                map.Put(i, TestSupport.RandomString());
                if (i == Count / 2)
                {
                    Cluster.RemoveNode(nodeId);
                }
            }
    
            TestSupport.AssertTrueEventually(() =>
            {
                var keys = map.KeySet();
                for (int i = 0; i < Count; i++)
                {
                    Assert.IsTrue(keys.Contains(i), "Key " + i + " was not found");
                }
                Assert.AreEqual(Count, map.Size());
            }, timeoutSeconds: 60);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ClientTransactionRetry()
        {
            var context = Client.NewTransactionContext();
            context.BeginTransaction();

            var map = context.GetMap<int, string>(Name);

            Cluster.RemoveNode();
            Cluster.AddNode();
            try
            {
                for (int i = 0; i < Count; i++)
                {
                    // put should eventually fail as the node which the transaction is running against 
                    // will be shut down
                    map.Put(i, TestSupport.RandomString());
                }
            }
            finally
            {
                context.RollbackTransaction();
            }
        }
    }
}
