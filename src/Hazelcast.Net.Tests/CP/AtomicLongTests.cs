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

using System.Threading.Tasks;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    [TestFixture]
    public class AtomicLongTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task Get()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            Assert.That(await along.GetAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task SetAndGet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task IncrementAndGet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.IncrementAndGetAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAsync(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAndIncrement()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAndIncrementAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAsync(), Is.EqualTo(2));
        }

        [Test]
        public async Task DecrementAndGet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(2);
            Assert.That(await along.GetAsync(), Is.EqualTo(2));
            Assert.That(await along.DecrementAndGetAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetAndDecrement()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(2);
            Assert.That(await along.GetAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAndDecrementAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task AddAndGet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.AddAndGetAsync(3), Is.EqualTo(4));
            Assert.That(await along.GetAsync(), Is.EqualTo(4));
        }

        [Test]
        public async Task GetAndAdd()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAndAddAsync(3), Is.EqualTo(1));
            Assert.That(await along.GetAsync(), Is.EqualTo(4));
        }

        [TestCase(2, 3, false)]
        [TestCase(1, 3, true)]
        public async Task CompareAndSet(int comparand, int value, bool result)
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.CompareAndSetAsync(comparand, value), Is.EqualTo(result));
            Assert.That(await along.GetAsync(), Is.EqualTo(result ? value : 1));
        }

        [Test]
        public async Task GetAndSet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAndSetAsync(3), Is.EqualTo(1));
            Assert.That(await along.GetAsync(), Is.EqualTo(3));
        }

        [Test]
        public async Task Name()
        {
            var name = CreateUniqueName();
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(name);

            Assert.That(along.Name, Is.EqualTo(name));
        }

        [Test]
        public async Task MultipleDestroy()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.DestroyAsync();
            await along.DestroyAsync();
        }

        [Test]
        public async Task AfterDestroy()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.DestroyAsync();

            var e = await AssertEx.ThrowsAsync<RemoteException>(async () => await along.SetAsync(1));
            Assert.That(e.Error, Is.EqualTo(RemoteError.DistributedObjectDestroyed));
        }
    }
}
