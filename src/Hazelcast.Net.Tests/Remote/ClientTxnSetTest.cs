﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    public class ClientTxnSetTest : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task TestAddRemove()
        {
            var set = await Client.GetSetAsync<string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            await set.AddAsync("item1");

            await using var context = await Client.BeginTransactionAsync();

            var txSet = await context.GetSetAsync<string>(set.Name);
            Assert.IsTrue(await txSet.AddAsync("item2"));
            Assert.AreEqual(2, await txSet.GetSizeAsync());
            Assert.AreEqual(1, await set.GetSizeAsync());
            Assert.IsFalse(await txSet.RemoveAsync("item3"));
            Assert.IsTrue(await txSet.RemoveAsync("item1"));
            await context.CommitAsync();
            Assert.AreEqual(1, await set.GetSizeAsync());
        }
    }
}
