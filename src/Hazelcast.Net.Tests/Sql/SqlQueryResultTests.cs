// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlQueryResultTests : SqlTestBase
    {
        [Test]
        public async Task EnumerateAfterDispose()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            var result = Client.Sql.ExecuteQuery($"SELECT * FROM {map.Name}");
            await result.DisposeAsync();

            Assert.ThrowsAsync<ObjectDisposedException>(async () => await result.MoveNextAsync());
            Assert.Throws<ObjectDisposedException>(() => result.EnumerateOnce());
            Assert.Throws<ObjectDisposedException>(() => result.EnumerateOnceAsync());
        }

        [Test]
        public async Task EnumerateMultipleTimes()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            var result = Client.Sql.ExecuteQuery($"SELECT * FROM {map.Name}");
            await result.MoveNextAsync();

            Assert.Throws<InvalidOperationException>(() => result.EnumerateOnce());
            Assert.Throws<InvalidOperationException>(() => result.EnumerateOnceAsync());
        }

        [Test]
        public async Task DisposeMultipleTimes()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            var result = Client.Sql.ExecuteQuery($"SELECT * FROM {map.Name}");
            await result.DisposeAsync();

            Assert.DoesNotThrowAsync(async () =>
            {
                await result.DisposeAsync();
                await result.DisposeAsync();
            });
        }
    }
}
