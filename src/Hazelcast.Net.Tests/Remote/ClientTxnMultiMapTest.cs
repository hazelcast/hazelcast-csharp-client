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

using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientTxMultiMapTest : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task TestPutGetRemove()
        {
            var multiDictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(multiDictionary);

            for (var i = 0; i < 10; i++)
            {
                var key = i + "key";
                await multiDictionary.TryAddAsync(key, "value");
                await using var context = await Client.BeginTransactionAsync();
                var txMultiDictionary = await context.GetTransactionalAsync(multiDictionary);
                Assert.IsFalse(await txMultiDictionary.TryAddAsync(key, "value"));
                Assert.IsTrue(await txMultiDictionary.TryAddAsync(key, "value1"));
                Assert.IsTrue(await txMultiDictionary.TryAddAsync(key, "value2"));
                Assert.AreEqual(3, (await txMultiDictionary.GetAsync(key)).Count);
                await context.CommitAsync();
                Assert.AreEqual(3, (await multiDictionary.GetAsync(key)).Count);
            }
        }

        [Test]
        public async Task TestPutGetRemove2()
        {
            var multiDictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            var key = "key";
            await multiDictionary.TryAddAsync(key, "value");
            var context = await Client.BeginTransactionAsync();

            var txMultiDictionary = await context.GetTransactionalAsync(multiDictionary);

            Assert.IsFalse(await txMultiDictionary.TryAddAsync(key, "value"));
            Assert.IsTrue(await txMultiDictionary.TryAddAsync(key, "value1"));
            Assert.IsTrue(await txMultiDictionary.TryAddAsync(key, "value2"));
            Assert.AreEqual(3, (await txMultiDictionary.GetAsync(key)).Count);

            await context.CommitAsync();

            Assert.AreEqual(3, (await multiDictionary.GetAsync(key)).Count);
        }

        [Test]
        public async Task TestRemove()
        {
            const string key = "key";
            const string value = "value";

            var multiDictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());

            await multiDictionary.TryAddAsync(key, value);
            var context = await Client.BeginTransactionAsync();

            var txMultiDictionary = await context.GetTransactionalAsync(multiDictionary);
            await txMultiDictionary.RemoveAsync(key, value);
            await context.CommitAsync();

            Assert.AreEqual(new List<string>(), await multiDictionary.GetAsync(key));
        }

        [Test]
        public async Task TestRemoveAll()
        {
            const string key = "key";
            const string value = "value";
            var multiDictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            for (var i = 0; i < 10; i++)
            {
                await multiDictionary.TryAddAsync(key, value + i);
            }


            var context = await Client.BeginTransactionAsync();

            var txMultiDictionary = await context.GetTransactionalAsync(multiDictionary);
            await txMultiDictionary.RemoveAsync(key);
            await context.CommitAsync();

            Assert.AreEqual(new List<string>(), await multiDictionary.GetAsync(key));
        }

        [Test]
        public async Task TestSize()
        {
            var key = "key";
            var value = "value";

            var multiDictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await multiDictionary.TryAddAsync(key, value);

            var context = await Client.BeginTransactionAsync();

            var txMultiDictionary = await context.GetTransactionalAsync(multiDictionary);

            await txMultiDictionary.TryAddAsync(key, "newValue");
            await txMultiDictionary.TryAddAsync("newKey", value);

            Assert.AreEqual(3, await txMultiDictionary.CountAsync());

            await context.CommitAsync();
        }

        [Test]
        public async Task TestValueCount()
        {
            var key = "key";
            var value = "value";

            var multiDictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await multiDictionary.TryAddAsync(key, value);

            var context = await Client.BeginTransactionAsync();
            var txMultiDictionary = await context.GetTransactionalAsync(multiDictionary);

            await txMultiDictionary.TryAddAsync(key, "newValue");

            Assert.AreEqual(2, await txMultiDictionary.ValueCountAsync(key));

            await context.CommitAsync();
        }
    }
}