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
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    [TestFixture]
    public class AtomicLongTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task Test()
        {
            var along = await Client.CPSubsystem.GetAtomicLongAsync("along");

            await along.SetAsync(1);
            Assert.That(await along.GetAsync(), Is.EqualTo(1));

            Assert.That(await along.IncrementAndGetAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAndIncrementAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAsync(), Is.EqualTo(3));

            Assert.That(await along.DecrementAndGetAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAndDecrementAsync(), Is.EqualTo(2));
            Assert.That(await along.GetAsync(), Is.EqualTo(1));

            Assert.That(await along.GetAndAddAsync(9), Is.EqualTo(1));
            Assert.That(await along.AddAndGetAsync(9), Is.EqualTo(19));
            Assert.That(await along.GetAsync(), Is.EqualTo(19));

            Assert.That(await along.CompareAndSetAsync(1, 20), Is.False);
            Assert.That(await along.CompareAndSetAsync(19, 20), Is.True);
            Assert.That(await along.GetAsync(), Is.EqualTo(20));

            await along.DestroyAsync();
            await along.DisposeAsync();
        }
    }
}
