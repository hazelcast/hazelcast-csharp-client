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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ValueTaskExTests : ObservingTestBase
    {
        [SetUp]
        [TearDown]
        public void Reset()
        {
            AsyncContext.Reset();
        }

        [Test]
        public async Task WithNewContext()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            long idx = -1;
            await ValueTaskEx.WithNewContext(() =>
            {
                idx = AsyncContext.CurrentContext.Id;
                return new ValueTask();
            });

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextResult()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            var idx = await ValueTaskEx.WithNewContext(() =>
                new ValueTask<long>(AsyncContext.CurrentContext.Id));

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextToken()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            long idx = -1;
            await ValueTaskEx.WithNewContext(token =>
            {
                idx = AsyncContext.CurrentContext.Id;
                return new ValueTask();
            }, CancellationToken.None);

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextResultToken()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            var idx = await ValueTaskEx.WithNewContext(token =>
                new ValueTask<long>(AsyncContext.CurrentContext.Id), CancellationToken.None);

            Assert.That(idx, Is.Not.EqualTo(id));
        }
    }
}