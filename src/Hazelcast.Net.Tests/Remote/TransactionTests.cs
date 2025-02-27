// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class TransactionTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task ExplicitCommitPattern()
        {
            await using var list = await Client.GetListAsync<string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(list);
            await list.AddAsync("item1");

            await using (var tx = await Client.BeginTransactionAsync())
            {
                var txList = await tx.GetListAsync<string>(list.Name);

                Assert.IsTrue(await txList.AddAsync("item2"));
                Assert.AreEqual(2, await txList.GetSizeAsync());
                Assert.AreEqual(1, await list.GetSizeAsync());
                Assert.IsFalse(await txList.RemoveAsync("item3"));
                Assert.IsTrue(await txList.RemoveAsync("item1"));
                Assert.IsTrue(await txList.AddAsync("item4"));

                await tx.CommitAsync();
            }

            Assert.AreEqual(2, await list.GetSizeAsync());
            var items = await list.GetAllAsync();
            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(items.Contains("item2"));
            Assert.IsTrue(items.Contains("item4"));
        }

        [Test]
        public async Task ExplicitRollbackPattern()
        {
            await using var list = await Client.GetListAsync<string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(list);
            await list.AddAsync("item1");

            await using (var tx = await Client.BeginTransactionAsync())
            {
                var txList = await tx.GetListAsync<string>(list.Name);

                Assert.IsTrue(await txList.AddAsync("item2"));
                Assert.AreEqual(2, await txList.GetSizeAsync());
                Assert.AreEqual(1, await list.GetSizeAsync());
                Assert.IsFalse(await txList.RemoveAsync("item3"));
                Assert.IsTrue(await txList.RemoveAsync("item1"));
                Assert.IsTrue(await txList.AddAsync("item4"));

                await tx.RollbackAsync();
            }

            Assert.AreEqual(1, await list.GetSizeAsync());
            var items = await list.GetAllAsync();
            Assert.AreEqual(1, items.Count);
            Assert.IsTrue(items.Contains("item1"));
        }

        [Test]
        public async Task ImplicitCommitPattern()
        {
            await using var list = await Client.GetListAsync<string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await list.AddAsync("item1");

            await using (var tx = await Client.BeginTransactionAsync())
            {
                var txList = await tx.GetListAsync<string>(list.Name);

                Assert.IsTrue(await txList.AddAsync("item2"));
                Assert.AreEqual(2, await txList.GetSizeAsync());
                Assert.AreEqual(1, await list.GetSizeAsync());
                Assert.IsFalse(await txList.RemoveAsync("item3"));
                Assert.IsTrue(await txList.RemoveAsync("item1"));
                Assert.IsTrue(await txList.AddAsync("item4"));

                tx.Complete();
            }

            Assert.AreEqual(2, await list.GetSizeAsync());
            var items = await list.GetAllAsync();
            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(items.Contains("item2"));
            Assert.IsTrue(items.Contains("item4"));
        }

        [Test]
        public async Task ImplicitRollbackPattern()
        {
            await using var list = await Client.GetListAsync<string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await list.AddAsync("item1");

            await using (var tx = await Client.BeginTransactionAsync())
            {
                var txList = await tx.GetListAsync<string>(list.Name);

                Assert.IsTrue(await txList.AddAsync("item2"));
                Assert.AreEqual(2, await txList.GetSizeAsync());
                Assert.AreEqual(1, await list.GetSizeAsync());
                Assert.IsFalse(await txList.RemoveAsync("item3"));
                Assert.IsTrue(await txList.RemoveAsync("item1"));
            }

            Assert.AreEqual(1, await list.GetSizeAsync());
            var items = await list.GetAllAsync();
            Assert.AreEqual(1, items.Count);
            Assert.IsTrue(items.Contains("item1"));
        }
    }
}
