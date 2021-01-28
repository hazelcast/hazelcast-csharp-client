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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class AsyncQueueTests
    {
        [Test]
        public void WritingCompletedQueueIsFalse()
        {
            var queue = new AsyncQueue<int>();

            queue.Complete();

            Assert.That(queue.TryWrite(1), Is.False);
        }

        [Test]
        public async Task WaitingCompletedQueueIsFalse()
        {
            var queue = new AsyncQueue<int>();

            queue.Complete();

            Assert.That(await queue.WaitAsync(), Is.False);
        }

        [Test]
        public async Task WritingQueueEndsWait()
        {
            var queue = new AsyncQueue<int>();

            var task = Task.Run(async () => await queue.WaitAsync());

            Assert.That(queue.TryWrite(1), Is.True);

            Assert.That(await task, Is.True);
            Assert.That(queue.Read(), Is.EqualTo(1));
        }

        [Test]
        public async Task CompletingQueueEndsWait()
        {
            var queue = new AsyncQueue<int>();

            var task = Task.Run(async () => await queue.WaitAsync());

            queue.Complete();

            Assert.That(await task, Is.False);
        }

        [Test]
        public async Task WriteThenRead()
        {
            var queue = new AsyncQueue<int>();

            queue.TryWrite(1);
            queue.TryWrite(2);
            queue.TryWrite(3);

            Assert.That(await queue.WaitAsync(), Is.True);
            Assert.That(queue.Read(), Is.EqualTo(1));

            Assert.That(await queue.WaitAsync(), Is.True);
            Assert.That(queue.Read(), Is.EqualTo(2));

            Assert.That(await queue.WaitAsync(), Is.True);
            Assert.That(queue.Read(), Is.EqualTo(3));
        }

        [Test]
        public async Task WriteAndRead()
        {
            var queue = new AsyncQueue<int>();

            queue.TryWrite(1);

            Assert.That(await queue.WaitAsync(), Is.True);
            Assert.That(queue.Read(), Is.EqualTo(1));

            queue.TryWrite(2);

            Assert.That(await queue.WaitAsync(), Is.True);
            Assert.That(queue.Read(), Is.EqualTo(2));

            queue.TryWrite(3);

            Assert.That(await queue.WaitAsync(), Is.True);
            Assert.That(queue.Read(), Is.EqualTo(3));
        }

        [Test]
        public async Task Enumerate()
        {
            var queue = new AsyncQueue<int>();
            var list = new List<int>();

            var task = Task.Run(async () =>
            {
                await foreach (var i in queue)
                    list.Add(i);
            });

            for (var i = 0; i < 100; i++)
                Assert.That(queue.TryWrite(i), Is.True);

            queue.Complete();
            await task;

            Assert.That(list.Count, Is.EqualTo(100));
        }

        [Test]
        public async Task StressTest()
        {
            var queue = new AsyncQueue<int>();
            var list = new List<int>();

            var task = Task.Run(async () =>
            {
                await foreach (var i in queue)
                    list.Add(i);
            });

            var writerCount = 10;
            var writers = new Task[writerCount];
            var indexes = new int[writerCount];
            var writing = true;
            var paused = false;
            for (var i = 0; i < writerCount; i++)
            {
                var n = i;
                writers[n] = Task.Run(async () =>
                {
                    while (writing)
                    {
                        if (!paused && queue.TryWrite(indexes[n]))
                            indexes[n]++;

                        await Task.Delay(RandomProvider.Random.Next(100));
                    }
                });
            }

            await Task.Delay(2000);
            paused = true;
            await Task.Delay(1000);
            paused = false;
            await Task.Delay(2000);

            writing = false;
            queue.Complete();
            await task;

            await Task.WhenAll(writers);

            var total = 0;
            for (var i = 0; i < writerCount; i++)
            {
                Console.WriteLine($"{i}: {indexes[i]}");
                total += indexes[i];
            }

            Assert.That(list.Count, Is.EqualTo(total));
        }
    }
}
