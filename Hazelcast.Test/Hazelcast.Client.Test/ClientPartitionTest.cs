using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hazelcast.Client.Spi;
using Hazelcast.IO;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientPartitionTest : HazelcastBaseTest
    {
        

        [Test]
        public void TestPartitionsUpdatedAfterNewNode()
        {
            var proxy = ((HazelcastClientProxy) Client);
            var partitionService = proxy.GetClient().GetClientPartitionService();

            var partitionCount = partitionService.GetPartitionCount();
            Assert.AreEqual(271, partitionCount);

            var owners = GetPartitionOwners(partitionCount, partitionService);
            Assert.AreEqual(1, owners.Count);

            var id = AddNodeAndWait();
            try
            {
                TestSupport.AssertTrueEventually(() =>
                {
                    owners = GetPartitionOwners(partitionCount, partitionService);
                    Assert.AreEqual(2, owners.Count);
                });
            }
            finally
            {
                RemoveNodeAndWait(id);

                TestSupport.AssertTrueEventually(() =>
                {
                    owners = GetPartitionOwners(partitionCount, partitionService);
                    Assert.AreEqual(1, owners.Count);
                });
            }
        }

        private static HashSet<Address> GetPartitionOwners(int partitionCount, IClientPartitionService partitionService)
        {
            var partitionOwners = new HashSet<Address>();
            for (int i = 0; i < partitionCount; i++)
            {
                partitionOwners.Add(partitionService.GetPartitionOwner(i));
            }
            return partitionOwners;
        }
    }
}
