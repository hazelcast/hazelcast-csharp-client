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

using System;
using System.Threading.Tasks;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    [TestFixture]
    public class AtomicReferenceTests: SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task TestName()
        {
            var name = CreateUniqueName();
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(name);

            Assert.That(aref.Name, Is.EqualTo(name));

            await aref.DestroyAsync();
        }

        [Test]
        public async Task TestGet()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());

            Assert.That(await aref.GetAsync(), Is.Null);

            await aref.DestroyAsync();
        }
        
        [Test]
        public async Task TestContains()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());

            await aref.SetAsync("val");
            Assert.True(await aref.ContainsAsync("val"));
            await aref.DestroyAsync();
        }
        
        [Test]
        public async Task TestClear()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());

            await aref.SetAsync("val");
            Assert.AreEqual("val",await aref.GetAsync());
            await aref.ClearAsync();
            Assert.That(await aref.GetAsync(), Is.Null.Or.Empty);
            Assert.True(await aref.IsNullAsync());
            await aref.DestroyAsync();
        }

        [Test]
        public async Task TestSetAndGet()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());
            var value = RandomString();

            await aref.SetAsync(value);
            Assert.That(await aref.GetAsync(), Is.EqualTo(value));

            await aref.DestroyAsync();
        }

        [Test]
        public async Task TestSetAndGetNull()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());

            var value = RandomString();
            await aref.SetAsync(value);
            Assert.That(await aref.GetAsync(), Is.EqualTo(value));

            await aref.SetAsync(null);
            Assert.That(await aref.GetAsync(), Is.Null);

            await aref.DestroyAsync();
        }

        [Test]
        public async Task TestGetAndSet()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());
            var (value1, value2) = (RandomString(), RandomString());

            await aref.SetAsync(value1);
            Assert.That(await aref.GetAsync(), Is.EqualTo(value1));
            Assert.That(await aref.GetAndSetAsync(value2), Is.EqualTo(value1));
            Assert.That(await aref.GetAsync(), Is.EqualTo(value2));

            await aref.DestroyAsync();
        }

        [TestCase("1", "22", "333", false)]
        [TestCase("1", "1", "333", true)]
        public async Task TestCompareAndSet(string initial, string comparand, string value, bool result)
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());

            await aref.SetAsync(initial);
            Assert.That(await aref.GetAsync(), Is.EqualTo(initial));
            Assert.That(await aref.CompareAndSetAsync(comparand, value), Is.EqualTo(result));
            Assert.That(await aref.GetAsync(), Is.EqualTo(result ? value : initial));

            await aref.DestroyAsync();
        }

        [Test]
        public async Task TestMultipleDestroy()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());

            await aref.DestroyAsync();
            await aref.DestroyAsync();
        }

        [Test]
        public async Task TestAfterDestroy()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicReferenceAsync<string>(CreateUniqueName());

            await aref.DestroyAsync();

            var e = await AssertEx.ThrowsAsync<RemoteException>(async () => await aref.SetAsync(RandomString()));
            Assert.That(e.Error, Is.EqualTo(RemoteError.DistributedObjectDestroyed));
        }

        private string RandomString() => Guid.NewGuid().ToString("N");
    }
}