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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientRingbufferTest : SingleMemberClientRemoteTestBase
    {
        // important to stick with this name as it is configured in hazelcast.xml
        // with a corresponding capacity of 6 items
        // (and ClientRingbufferTestWithTTL with a 180s ttl)
        private const string RingBufferNameBase = "ClientRingbufferTest";

        // important to match what's in hazelcast.xml
        private const int Capacity = 10;

        [Test]
        public async Task TestAddAll()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            var s = await rb.AddAllAsync(new List<string> {"foo", "bar"}, OverflowPolicy.Overwrite);

            Assert.AreEqual(s, await rb.GetTailSequenceAsync());
            Assert.AreEqual("foo", await rb.ReadOneAsync(0));
            Assert.AreEqual("bar", await rb.ReadOneAsync(1));
            Assert.AreEqual(0, await rb.GetHeadSequenceAsync());
            Assert.AreEqual(1, await rb.GetTailSequenceAsync());
        }

        [Test]
        public async Task TestAddAndReadOne()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            var sequence = await rb.AddAsync("foo");

            Assert.AreEqual("foo", await rb.ReadOneAsync(sequence));
        }

        [Test]
        public async Task TestAddAsync()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            var s = await rb.AddAsync("foo", OverflowPolicy.Overwrite);

            Assert.AreEqual("foo", await rb.ReadOneAsync(s));
        }

        [Test]
        public async Task TestCapacity()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            Assert.AreEqual(Capacity, await rb.GetCapacityAsync());
        }

        [Test]
        [Timeout(20_000)]
		public async Task TestExcessiveMaxCount()
		{
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            await AssertEx.ThrowsAsync<ArgumentException>(async () =>
            {
                await rb.ReadManyAsync(0, 0, rb.MaxBatchSize + 1);
            });
		}

        [Test]
        [Timeout(10_000)]
        public async Task TestExcessiveMinCount()
		{
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            await AssertEx.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                // so with this model, the invocation task

                await rb.ReadManyAsync(0, Capacity + 1, Capacity + 1).CfAwait();
            });
        }

        [Test]
        public async Task TestHeadSequence()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            for (var k = 0; k < 2*Capacity; k++)
            {
                await rb.AddAsync("foo");
            }

            Assert.AreEqual(Capacity, await rb.GetHeadSequenceAsync());
        }

        [Test]
		public async Task TestInvalidReadCount()
		{
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await rb.ReadManyAsync(0, 2, 1);
            });
		}

        [Test]
		public async Task TestInvalidSequence()
		{
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await rb.ReadOneAsync(-1);
            });
		}

        [Test]
        public async Task TestReadManyAsync()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            await rb.AddAsync("1");
            await rb.AddAsync("2");
            await rb.AddAsync("3");

            var result = await rb.ReadManyAsync(0, 3, 3);
            Assert.That(result, Is.EquivalentTo(new[] { "1", "2", "3" }));
        }

        [Test]
        public async Task TestReadManyAsyncWithMaxCount()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            await rb.AddAsync("1");
            await rb.AddAsync("2");
            await rb.AddAsync("3");
            await rb.AddAsync("4");
            await rb.AddAsync("5");
            await rb.AddAsync("6");

            //surplus results should not be read
            var result = await rb.ReadManyAsync(0, 3, 3);
            Assert.That(result, Is.EquivalentTo(new[] { "1", "2", "3" }));
        }

        [Test]
        public async Task TestRemainingCapacity()
        {
            var rb = await Client.GetRingBufferAsync<string>("ClientRingbufferTestWithTTL" + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            await rb.AddAsync("foo");

            Assert.AreEqual(Capacity - 1, await rb.GetRemainingCapacityAsync());

            // meh? this is not testing ttl at all?
        }

        [Test]
        public async Task TestSize()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            await rb.AddAsync("foo");

            Assert.AreEqual(1, await rb.GetSizeAsync());
        }

        [Test]
		public async Task TestStaleSequence()
		{
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            for (var k = 0; k < Capacity * 2; k++)
            {
                await rb.AddAsync("foo");
            }

            try
            {
                await rb.ReadOneAsync(await rb.GetHeadSequenceAsync() - 1);
                Assert.Fail("Expected an exception.");
            }
            catch (RemoteException e)
            {
                Assert.That(e.Error, Is.EqualTo(RemoteError.StaleSequence));
            }
		}

        [Test]
        public async Task TestTailSequence()
        {
            var rb = await Client.GetRingBufferAsync<string>(RingBufferNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(rb);

            for (var k = 0; k < 2*Capacity; k++)
            {
                await rb.AddAsync("foo");
            }

            Assert.AreEqual(Capacity*2 - 1, await rb.GetTailSequenceAsync());
        }
    }
}
