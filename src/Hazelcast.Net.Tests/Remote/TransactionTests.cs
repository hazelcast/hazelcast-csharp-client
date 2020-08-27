// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class TransactionTests : SingleMemberRemoteTestBase
    {
        [Test]
        public async Task Test()
        {
            await using var client = await CreateAndStartClientAsync().CAF();
            await using var list = await client.GetListAsync<string>(CreateUniqueName());
            await list.AddAsync("item1");

            await using (var tx = await client.BeginTransactionAsync())
            {
                var txList = await tx.GetListAsync(list);

                NUnit.Framework.Assert.IsTrue(await txList.AddAsync("item2"));
                NUnit.Framework.Assert.AreEqual(2, await txList.CountAsync());
                NUnit.Framework.Assert.AreEqual(1, await list.CountAsync());
                NUnit.Framework.Assert.IsFalse(await txList.RemoveAsync("item3"));
                NUnit.Framework.Assert.IsTrue(await txList.RemoveAsync("item1"));

                // TODO: consider working same as System.Transaction
                await tx.CommitAsync();
            }

            NUnit.Framework.Assert.AreEqual(1, await list.CountAsync());
            var items = await list.GetAsync();
            NUnit.Framework.Assert.AreEqual(1, items.Count);
            NUnit.Framework.Assert.IsTrue(items.Contains("item2"));

            await client.DestroyAsync(list).CAF();

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
