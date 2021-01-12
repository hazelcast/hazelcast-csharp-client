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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class AsyncContextTests
    {
        [SetUp]
        public void SetUp()
        {
            // each test starts with a fresh state
            // no context, reset sequence
            AsyncContext.ClearCurrent();
            AsyncContext.ResetSequence();
        }


        [Test]
        public void CreatesCurrentContext()
        {
            Assert.That(AsyncContext.HasCurrent, Is.False);

            var context = AsyncContext.CurrentContext;
            Assert.That(context, Is.Not.Null);
            Assert.That(AsyncContext.CurrentContext, Is.SameAs(context));
            Assert.That(AsyncContext.HasCurrent, Is.True);
        }

        [Test]
        public async Task CreatesContextsInFlows()
        {
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
            Assert.That(AsyncContext.CurrentContext.Id, Is.EqualTo(1));

            var id1 = await Task.Run(() => AsyncContext.CurrentContext.Id);
            var id2 = await AsyncContext.RunWithNew(() => Task.FromResult(AsyncContext.CurrentContext.Id));
            var id3 = await AsyncContext.RunWithNew(() => Task.Run(() => AsyncContext.CurrentContext.Id));
            var id4 = await AsyncContext.RunWithNew(token => Task.Run(() => AsyncContext.CurrentContext.Id, token), CancellationToken.None);
            var id5 = await Task.Run(() => AsyncContext.CurrentContext.Id);

            Assert.That(id1, Is.EqualTo(1));
            Assert.That(id2, Is.EqualTo(2));
            Assert.That(id3, Is.EqualTo(3));
            Assert.That(id4, Is.EqualTo(4));
            Assert.That(id5, Is.EqualTo(1));

            Assert.That(AsyncContext.CurrentContext.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task WithNewContextExceptions()
        {
            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await AsyncContext.RunWithNew(null));
            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await AsyncContext.RunWithNew(null, default));
        }

        [Test]
        public void InTransaction()
        {
            AsyncContext.Ensure();
            AsyncContext.CurrentContext.InTransaction = true;
            Assert.That(AsyncContext.CurrentContext.InTransaction, Is.True);
            AsyncContext.CurrentContext.InTransaction = false;
            Assert.That(AsyncContext.CurrentContext.InTransaction, Is.False);
        }

        [Test]
        public async Task AsyncContextWhenAsync()
        {
            // TODO: use manual reset events instead of timers

            Console.WriteLine("There is no ambient context, so each task + continuation creates its own.");

            long x1 = 0, x2 = 0, x3 = 0;

            await AsyncContextWhenAsyncSub("x1")
                .ContinueWith(x =>
                {
                    x1 = x.Result;
                    x2 = AsyncContext.CurrentContext.Id;
                    Console.WriteLine($"-x2: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");
                })
                .ContinueWith(async x => x3 = await AsyncContextWhenAsyncSub("x3"));

            Assert.AreNotEqual(x1, x2);
            Assert.AreNotEqual(x1, x3);
            Assert.AreNotEqual(x2, x3);

            await Task.Delay(500); // complete above code
            Console.WriteLine("--");


            Console.WriteLine("If we ensure a context (as HazelcastClient does) then we have a context that flows.");

            // first time we get a context = creates a context
            var z1 = AsyncContext.CurrentContext.Id;
            Console.WriteLine($"-z1: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");

            await Task.Yield();

            var z2 = AsyncContext.CurrentContext.Id;
            Console.WriteLine($"-z2: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");

            Assert.Greater(z1, x3);
            Assert.AreEqual(z1, z2);

            await Task.Delay(500); // complete above code
            Console.WriteLine("--");

            Console.WriteLine("Running detached code creates a new context, which flows.");

            long c1 = 0, c2 = 0, c4 = 0;

            var cx = await AsyncContext.RunWithNew(async () =>
            {
                return await AsyncContextWhenAsyncSub("c1").ContinueWith(async x =>
                {
                    c1 = x.Result;
                    c2 = await AsyncContextWhenAsyncSub("c2");
                    return c2;
                }).Unwrap();
            });

            await Task.Delay(500); // complete above code
            Console.WriteLine("-cx: [  ] " + cx);
            Console.WriteLine("--");

            Task<long> task = null;
            await AsyncContext.RunWithNew(() => task = AsyncContextWhenAsyncSub("c3"));

            Console.WriteLine("--");
            var c3 = await task;

            _ = AsyncContext.RunWithNew(async () =>
            {
                await Task.Delay(100);
                c4 = await AsyncContextWhenAsyncSub("c4");
            });

            await Task.Delay(500); // complete above code

            Assert.AreEqual(cx, c1);
            Assert.Greater(c1, z2);
            Assert.AreEqual(c1, c2);
            Assert.Greater(c3, c1);
            Assert.Greater(c4, c3);

            Console.WriteLine("--");

            Console.WriteLine("This operation is still in the same context");
            var z3 = AsyncContext.CurrentContext.Id;
            Console.WriteLine($"-z3: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");
            Assert.AreEqual(z1, z3);

            await Task.Delay(500);
        }

        private static async Task<long> AsyncContextWhenAsyncSub(string n)
        {
            var id = AsyncContext.CurrentContext.Id;
            Console.WriteLine($">{n}: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");

            async Task F(long i)
            {
                Assert.AreEqual(i, AsyncContext.CurrentContext.Id);
                Console.WriteLine($">{n}:   [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");
                await Task.Delay(10);
                Assert.AreEqual(i, AsyncContext.CurrentContext.Id);
                Console.WriteLine($">{n}:   [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");
            }

            await Task.Delay(10);
            await F(id);
            await Task.Delay(10);

            Assert.AreEqual(id, AsyncContext.CurrentContext.Id);
            Console.WriteLine($">{n}: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");

            return id;
        }
    }
}
