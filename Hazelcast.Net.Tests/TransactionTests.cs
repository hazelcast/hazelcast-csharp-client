using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class TransactionTests : HazelcastTestBase
    {
        [Test]
        public async Task Test()
        {
            var options = new HazelcastOptions();
            var clientFactory = new HazelcastClientFactory(options);
            await using var client = clientFactory.CreateClient(); // disposed at end of test
            await client.OpenAsync();

            var list = await client.GetListAsync<string>(CreateUniqueName());
            await list.AddAsync("item1");

            await using (var tx = await client.BeginTransactionAsync())
            {
                var txList = await tx.GetListAsync(list);

                NUnit.Framework.Assert.IsTrue(await txList.AddAsync("item2", default));
                NUnit.Framework.Assert.AreEqual(2, await txList.CountAsync(default));
                NUnit.Framework.Assert.AreEqual(1, await list.CountAsync());
                NUnit.Framework.Assert.IsFalse(await txList.RemoveAsync("item3", default));
                NUnit.Framework.Assert.IsTrue(await txList.RemoveAsync("item1", default));

                // TODO: consider working same as System.Transaction
                await tx.CommitAsync();
            }

            NUnit.Framework.Assert.AreEqual(1, await list.CountAsync());
            var items = await list.GetAllAsync();
            NUnit.Framework.Assert.AreEqual(1, items.Count);
            NUnit.Framework.Assert.IsTrue(items.Contains("item2"));

            list.Destroy(); // FIXME that should be async too + it's not implemented
            // but ... other than that... the test runs ok! ;)

            // original code from ClientTxnListTest.cs
            /*
            var name = TestSupport.RandomString();
            list = Client.GetList<object>(name);
            list.Add("item1");
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var listTx = context.GetList<object>(name);
            Assert.IsTrue(listTx.Add("item2"));
            Assert.AreEqual(2, listTx.Size());
            Assert.AreEqual(1, list.Count);
            Assert.IsFalse(listTx.Remove("item3"));
            Assert.IsTrue(listTx.Remove("item1"));
            context.CommitTransaction();
            Assert.AreEqual(1, list.Count);
            listTx.Destroy();
            */
        }
    }
}
