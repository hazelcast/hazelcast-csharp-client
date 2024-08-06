// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.CP;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    [TestFixture]
    public class AtomicLongTests : MultiMembersRemoteTestBase
    {
        protected override string RcClusterConfiguration => TestFiles.ReadAllText(this, "Cluster/cp.xml");

        protected IHazelcastClient Client;
        
        [SetUp]
        public async Task SetUp()
        {
            // CP-subsystem wants at least 3 members
            for (var i = 0; i < 3; i++) await AddMember();
            
            Client = await CreateAndStartClientAsync();
        }
        
        [Test]
        public async Task Get()
        {
            var cpSubSystem = (CPSubsystem) Client.CPSubsystem;
            await using var along = await cpSubSystem.GetAtomicLongAsync(CreateUniqueName());

            Assert.That(await along.GetAsync(), Is.EqualTo(0));

            await along.DestroyAsync();
        }
        
        [Test]
        public async Task GetWithGroup()
        {
            var groupName = CreateUniqueName();
            var objectName = CreateUniqueName() + "@"+ groupName;
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(objectName);

            Assert.That(along.GroupId.Name, Is.EqualTo(groupName));
            Assert.That(await along.GetAsync(), Is.EqualTo(0));

            await along.DestroyAsync();
        }

        [Test]
        public async Task SetAndGet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));

            await along.DestroyAsync();
        }

        [Test]
        public async Task IncrementAndGet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.IncrementAndGetAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAsync(), Is.EqualTo(2));

            await along.DestroyAsync();
        }

        [Test]
        public async Task GetAndIncrement()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAndIncrementAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAsync(), Is.EqualTo(2));

            await along.DestroyAsync();
        }

        [Test]
        public async Task DecrementAndGet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(2);
            Assert.That(await along.GetAsync(), Is.EqualTo(2));
            Assert.That(await along.DecrementAndGetAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAsync(), Is.EqualTo(1));

            await along.DestroyAsync();
        }

        [Test]
        public async Task GetAndDecrement()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(2);
            Assert.That(await along.GetAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAndDecrementAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAsync(), Is.EqualTo(1));

            await along.DestroyAsync();
        }

        [Test]
        public async Task AddAndGet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.AddAndGetAsync(3), Is.EqualTo(4));
            Assert.That(await along.GetAsync(), Is.EqualTo(4));

            await along.DestroyAsync();
        }

        [Test]
        public async Task GetAndAdd()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAndAddAsync(3), Is.EqualTo(1));
            Assert.That(await along.GetAsync(), Is.EqualTo(4));

            await along.DestroyAsync();
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

            await along.DestroyAsync();
        }

        [Test]
        public async Task GetAndSet()
        {
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(CreateUniqueName());

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));
            Assert.That(await along.GetAndSetAsync(3), Is.EqualTo(1));
            Assert.That(await along.GetAsync(), Is.EqualTo(3));

            await along.DestroyAsync();
        }

        [Test]
        public async Task Name()
        {
            var name = CreateUniqueName();
            await using var along = await Client.CPSubsystem.GetAtomicLongAsync(name);

            Assert.That(along.Name, Is.EqualTo(name));

            await along.DestroyAsync();
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
