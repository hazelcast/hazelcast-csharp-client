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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
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

            var context = AsyncContext.Current;
            Assert.That(context, Is.Not.Null);
            Assert.That(AsyncContext.Current, Is.SameAs(context));
            Assert.That(AsyncContext.HasCurrent, Is.True);
        }

        [Test]
        public async Task CreatesContextsInFlows()
        {
            Assert.That(AsyncContext.HasCurrent, Is.False);

            var id1 = await Task.Run(() => AsyncContext.Current.Id);
            var id2 = await Task.Run(() => AsyncContext.Current.Id);

            Assert.That(id1, Is.EqualTo(1));
            Assert.That(id2, Is.EqualTo(2));

            Assert.That(AsyncContext.HasCurrent, Is.False);
        }

        [Test]
        public async Task FlowsWithAsync()
        {
            Assert.That(AsyncContext.Current.Id, Is.EqualTo(1));

            var id1 = await Task.Run(() => AsyncContext.Current.Id);
            var id2 = await Task.Run(() => AsyncContext.Current.Id);

            Assert.That(id1, Is.EqualTo(1));
            Assert.That(id2, Is.EqualTo(1));

            Assert.That(AsyncContext.Current.Id, Is.EqualTo(1));
        }

        [Test]
        public void InTransaction()
        {
            AsyncContext.Ensure();
            AsyncContext.Current.InTransaction = true;
            Assert.That(AsyncContext.Current.InTransaction, Is.True);
            AsyncContext.Current.InTransaction = false;
            Assert.That(AsyncContext.Current.InTransaction, Is.False);
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
                    x2 = AsyncContext.Current.Id;
                    Console.WriteLine($"-x2: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.Current.Id}");
                })
                .ContinueWith(async x => x3 = await AsyncContextWhenAsyncSub("x3"));

            Assert.AreNotEqual(x1, x2);
            Assert.AreNotEqual(x1, x3);
            Assert.AreNotEqual(x2, x3);

            await Task.Delay(500); // complete above code
            Console.WriteLine("--");


            Console.WriteLine("If we ensure a context (as HazelcastClient does) then we have a context that flows.");

            // first time we get a context = creates a context
            var z1 = AsyncContext.Current.Id;
            Console.WriteLine($"-z1: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.Current.Id}");

            await Task.Yield();

            var z2 = AsyncContext.Current.Id;
            Console.WriteLine($"-z2: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.Current.Id}");

            Assert.Greater(z1, x3);
            Assert.AreEqual(z1, z2);

            await Task.Delay(500); // complete above code
            Console.WriteLine("--");

            Console.WriteLine("Running detached code creates a new context, which flows.");

            long c1 = 0, c2 = 0, c4 = 0;

            long cx;
            using (AsyncContext.New())
            {
                cx = await AsyncContextWhenAsyncSub("c1").ContinueWith(async x =>
                {
                    c1 = x.Result;
                    c2 = await AsyncContextWhenAsyncSub("c2");
                    return c2;
                }).Unwrap();
            };

            await Task.Delay(500); // complete above code
            Console.WriteLine("-cx: [  ] " + cx);
            Console.WriteLine("--");

            Task<long> task = null;
            using (AsyncContext.New())
            {
                task = AsyncContextWhenAsyncSub("c3");
            }

            Console.WriteLine("--");
            var c3 = await task;

            using (AsyncContext.New())
            {
                await Task.Delay(100);
                c4 = await AsyncContextWhenAsyncSub("c4");
            };

            await Task.Delay(500); // complete above code

            Assert.AreEqual(cx, c1);
            Assert.Greater(c1, z2);
            Assert.AreEqual(c1, c2);
            Assert.Greater(c3, c1);
            Assert.Greater(c4, c3);

            Console.WriteLine("--");

            Console.WriteLine("This operation is still in the same context");
            var z3 = AsyncContext.Current.Id;
            Console.WriteLine($"-z3: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.Current.Id}");
            Assert.AreEqual(z1, z3);

            await Task.Delay(500);
        }

        private static async Task<long> AsyncContextWhenAsyncSub(string n)
        {
            var id = AsyncContext.Current.Id;
            Console.WriteLine($">{n}: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.Current.Id}");

            async Task F(long i)
            {
                Assert.AreEqual(i, AsyncContext.Current.Id);
                Console.WriteLine($">{n}:   [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.Current.Id}");
                await Task.Delay(10);
                Assert.AreEqual(i, AsyncContext.Current.Id);
                Console.WriteLine($">{n}:   [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.Current.Id}");
            }

            await Task.Delay(10);
            await F(id);
            await Task.Delay(10);

            Assert.AreEqual(id, AsyncContext.Current.Id);
            Console.WriteLine($">{n}: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.Current.Id}");

            return id;
        }

        [Test]
        public async Task Test()
        {
            bool stop = false;
            var id = AsyncContext.Current.Id;

            // context is captured by the task and does *not* change
            var task = GetIds();

            using (AsyncContext.New())
            {
                await Task.Delay(1000);
            }

            stop = true;
            var ids = await task;
            foreach (var i in ids) Assert.That(i, Is.EqualTo(id));

            async Task<List<long>> GetIds()
            {
                var list = new List<long>();
                while (!stop)
                {
                    list.Add(AsyncContext.Current.Id);
                    await Task.Delay(100).CfAwait();
                }
                return list;
            }
        }
    }
}
