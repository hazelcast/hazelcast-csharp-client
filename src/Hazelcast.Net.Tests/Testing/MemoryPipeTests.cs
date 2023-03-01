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

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Testing.Networking;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    public class MemoryPipeTests
    {
        [Test]
        public void WriteOneReadOne()
        {
            var pipe = new MemoryPipe();

            const string s = "hello, world";

            for (var i = 0; i < 100; i++)
            {
                var bytes1 = Encoding.UTF8.GetBytes(s);
                pipe.Stream1.Write(bytes1, 0, bytes1.Length);

                var bytes2 = new byte[32];
                var count = pipe.Stream2.Read(bytes2, 0, bytes2.Length);

                Assert.That(count, Is.EqualTo(12), $"at i={i}");
                Assert.That(Encoding.UTF8.GetString(bytes2, 0, count), Is.EqualTo(s), $"at i={i}");
            }
        }

        [Test]
        public void WriteAllReadAll()
        {
            var pipe = new MemoryPipe();

            const string s = "hello, world";

            for (var i = 0; i < 100; i++)
            {
                var bytes1 = Encoding.UTF8.GetBytes(s);
                pipe.Stream1.Write(bytes1, 0, bytes1.Length);
            }

            for (var i = 0; i < 100; i++)
            {
                var bytes2 = new byte[32];
                var count = pipe.Stream2.Read(bytes2, 0, 12);

                Assert.That(count, Is.EqualTo(12), $"at i={i}");
                Assert.That(Encoding.UTF8.GetString(bytes2, 0, count), Is.EqualTo(s), $"at i={i}");
            }
        }

        [Test]
        public void WaitForWrite()
        {
            var pipe = new MemoryPipe();

            var count = 0;

            var t = new Thread(() =>
            {
                var bytes2 = new byte[32];
                count = pipe.Stream2.Read(bytes2, 0, 12);
            });

            t.Start();
            Thread.Sleep(1000);

            Assert.That(count, Is.Zero);

            const string s = "hello, world";
            var bytes1 = Encoding.UTF8.GetBytes(s);
            pipe.Stream1.Write(bytes1, 0, bytes1.Length);

            t.Join();
            Assert.That(count, Is.EqualTo(12));
        }

        [Test]
        public async Task WaitForWriteAsync()
        {
            var pipe = new MemoryPipe();

            var count = 0;

            var t = Task.Run(async () =>
            {
                var bytes2 = new byte[32];
                count = await pipe.Stream2.ReadAsync(bytes2, 0, 12);
            });

            await Task.Delay(1000);

            Assert.That(count, Is.Zero);

            const string s = "hello, world";
            var bytes1 = Encoding.UTF8.GetBytes(s);
            await pipe.Stream1.WriteAsync(bytes1, 0, bytes1.Length);

            await t;
            Assert.That(count, Is.EqualTo(12));
        }

        [Test]
        public async Task WaitForWriteAsync2()
        {
            var pipe = new MemoryPipe();

            const string s = "hello, world";
            var bytes1 = Encoding.UTF8.GetBytes(s);
            await pipe.Stream1.WriteAsync(bytes1, 0, bytes1.Length);
            var bytes2 = new byte[32];
            await pipe.Stream2.ReadAsync(bytes2, 0, 12);

            var count = 0;

            var t = Task.Run(async () =>
            {
                count = await pipe.Stream2.ReadAsync(bytes2, 0, 12);
            });

            await Task.Delay(1000);

            Assert.That(count, Is.Zero);

            await pipe.Stream1.WriteAsync(bytes1, 0, bytes1.Length);

            await t;
            Assert.That(count, Is.EqualTo(12));
        }
    }
}
