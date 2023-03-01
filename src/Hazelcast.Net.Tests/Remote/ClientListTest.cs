// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.DistributedObjects;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientListTest : ClientCollectionTestBase
    {
        private const string ListNameBase = "List";

        protected override async Task<IHCollection<string>> GetHCollectionAsync(string baseName = default, bool isUnique = true)
        {
            var name = baseName ?? ListNameBase;
            if (isUnique) name += "_" + CreateUniqueName();

            return await Client.GetListAsync<string>(name);
        }

        [Test]
        public async Task TestInsertAsync()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await FillCollection(list, 3);

            await list.AddAsync(1, "item1Mod");
            Assert.AreEqual("item1Mod", await list.GetAsync(1));
        }

        [Test]
        public async Task TestInsertRangeAsync()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await FillCollection(list, 3);

            await list.AddAllAsync(1, new[] {"item1Mod", "item2Mod"});
            Assert.That(new [] {"item0", "item1Mod", "item2Mod", "item1", "item2"},
                Is.EquivalentTo(await list.GetAllAsync()));
        }

        [Test]
        public async Task TestSetAsync()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await FillCollection(list, 5);

            await list.Set(3, "item3Mod");
            Assert.AreEqual("item3Mod", await list.GetAsync(3));
        }

        [Test]
        public async Task TestGetAsync()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await FillCollection(list, 5);

            Assert.AreEqual("item3", await list.GetAsync(3));
        }

        [Test]
        public async Task TestGetRangeAsync()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await FillCollection(list, 5);

            Assert.AreEqual("item3", await list.GetAsync(3));

            await list.GetSublistAsync(2, 4);
            Assert.That(new [] {"item2", "item3"}, Is.EquivalentTo(await list.GetSublistAsync(2, 4)));
        }

        [Test]
        public async Task TestIndexOfAsync()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            var items  = await FillCollection(list, 5);
            await list.AddAsync("item0");
            items.Add("item0");

            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(i, await list.IndexOfAsync(items[i]));
            }
        }

        [Test]
        public async Task LastIndexOfAsync()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await FillCollection(list, 5);
            var items  = await FillCollection(list, 5);

            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(i + 5 , await list.LastIndexOfAsync(items[i]));
            }
        }

        [Test]
        public async Task TestRemoveAtAsync()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await FillCollection(list, 5);

            Assert.False(await list.RemoveAsync("item33"));
            Assert.True(await list.RemoveAsync("item3"));
            Assert.AreEqual("item0", await list.RemoveAsync(0));
        }
    }
}
