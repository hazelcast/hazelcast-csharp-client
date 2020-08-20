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

using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class AsyncContextTests
    {
        [Test]
        public async Task CreateCurrentContext()
        {
            await Task.Yield(); // ensure it's async, else creates an app-wide context

            AsyncContext.Reset();

            Assert.That(AsyncContext.HasCurrent, Is.False);

            var context = AsyncContext.CurrentContext;
            Assert.That(context, Is.Not.Null);
            Assert.That(AsyncContext.CurrentContext, Is.SameAs(context));
            Assert.That(AsyncContext.HasCurrent, Is.True);
        }

        [Test]
        public async Task StartsWithNoContext2()
        {
            AsyncContext.Reset();

            // how can this be true?
            Assert.That(AsyncContext.HasCurrent, Is.False);

            var id1 = await Task.Run(() => AsyncContext.CurrentContext.Id);
            var id2 = await Task.Run(() => AsyncContext.CurrentContext.Id);

            Assert.That(id1, Is.EqualTo(1));
            Assert.That(id2, Is.EqualTo(2));

            Assert.That(AsyncContext.HasCurrent, Is.False);
        }

        [Test]
        public async Task FlowsWithAsync()
        {
            AsyncContext.Reset();
            AsyncContext.Ensure();

            Assert.That(AsyncContext.HasCurrent, Is.True);
            Assert.That(AsyncContext.CurrentContext.Id, Is.EqualTo(1));

            var id1 = await Task.Run(() => AsyncContext.CurrentContext.Id);
            var id2 = await Task.Run(() => AsyncContext.CurrentContext.Id);

            Assert.That(id1, Is.EqualTo(1));
            Assert.That(id2, Is.EqualTo(1));

            Assert.That(AsyncContext.CurrentContext.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task WithNewContext()
        {
            AsyncContext.Reset();
            AsyncContext.Ensure();

            Assert.That(AsyncContext.HasCurrent, Is.True);
            Assert.That(AsyncContext.CurrentContext.Id, Is.EqualTo(1));

            var id1 = await Task.Run(() => AsyncContext.CurrentContext.Id);
            var id2 = await TaskEx.WithNewContext(() => Task.FromResult(AsyncContext.CurrentContext.Id));
            var id3 = await TaskEx.WithNewContext(() => Task.Run(() => AsyncContext.CurrentContext.Id));
            var id4 = await TaskEx.WithNewContext(token => Task.Run(() => AsyncContext.CurrentContext.Id, token), CancellationToken.None);
            var id5 = await Task.Run(() => AsyncContext.CurrentContext.Id);

            Assert.That(id1, Is.EqualTo(1));
            Assert.That(id2, Is.EqualTo(2));
            Assert.That(id3, Is.EqualTo(3));
            Assert.That(id4, Is.EqualTo(4));
            Assert.That(id5, Is.EqualTo(1));

            Assert.That(AsyncContext.CurrentContext.Id, Is.EqualTo(1));
        }

        [Test]
        public void WithNewContextExceptions()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => TaskEx.WithNewContext(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => TaskEx.WithNewContext(null, default));
        }

        [Test]
        public async Task InTransaction()
        {
            await Task.Yield(); // ensure it's async, else creates an app-wide context

            AsyncContext.Ensure();
            AsyncContext.CurrentContext.InTransaction = true;
            Assert.That(AsyncContext.CurrentContext.InTransaction, Is.True);
            AsyncContext.CurrentContext.InTransaction = false;
            Assert.That(AsyncContext.CurrentContext.InTransaction, Is.False);
        }
    }
}
