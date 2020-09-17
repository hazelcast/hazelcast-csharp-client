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

using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientTxnListTest : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task TestAddRemove()
        {
            var list = await Client.GetListAsync<string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await list.AddAsync("item1");

            await using var context = await Client.BeginTransactionAsync();

            var txList = await context.GetTransactionalAsync(list);
            Assert.IsTrue(await txList.AddAsync("item2"));
            Assert.AreEqual(2, await txList.CountAsync());
            Assert.AreEqual(1, await list.CountAsync());
            Assert.IsFalse(await txList.RemoveAsync("item3"));
            Assert.IsTrue(await txList.RemoveAsync("item1"));
            await context.CommitAsync();
            Assert.AreEqual(1, await list.CountAsync());
        }
    }
}