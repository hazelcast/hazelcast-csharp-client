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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.NetStandard
{
    [TestFixture]
    public class StreamExtensionsTests : ObservingTestBase
    {
        [Test]
        public async Task ReadAsync()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit."));
            var memory = new Memory<byte>(new byte[8]);

#if NETFRAMEWORK
            // for NETSTANDARD ReadAsync is a real method, not an extension
            Assert.ThrowsAsync<ArgumentNullException>(async () => _ = await ((Stream) null).ReadAsync(memory, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentException>(async () => _ = await stream.ReadAsync(default, CancellationToken.None));
#endif

            var count = await stream.ReadAsync(memory, CancellationToken.None);
            Assert.That(count, Is.EqualTo(8));
            Assert.That(Encoding.UTF8.GetString(memory.ToArray()), Is.EqualTo("Lorem ip"));

            stream = new MemoryStream(Encoding.UTF8.GetBytes("Lorem."));
            memory = new Memory<byte>(new byte[16]);

            count = await stream.ReadAsync(memory, CancellationToken.None);
            Assert.That(count, Is.EqualTo(6));
            Assert.That(Encoding.UTF8.GetString(memory.ToArray(), 0, count), Is.EqualTo("Lorem."));

            stream = new MemoryStream(Encoding.UTF8.GetBytes("Lorem."));
            memory = new Memory<byte>(new byte[16]);

            // for memory streams, the read operation itself cancels
            Assert.ThrowsAsync<TaskCanceledException>(async () => { await stream.ReadAsync(memory, new CancellationToken(true)); });
        }

        [Test]
        [Timeout(10_000)]
        public async Task ReadAsyncNetworkStream()
        {
            // but for network streams, it's different - they somehow can ignore the cancellation
            // test using a special stream that ignores the cancellation, too.
            // not changing the result of the test, but covering code that would otherwise be non-covered.

            var stream = new TestStream(Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit."));
            var memory = new Memory<byte>(new byte[8]);

            var task = stream.ReadAsync(memory, new CancellationToken(true));
            await Task.Delay(100);
            stream.CompleteRead();

            Assert.ThrowsAsync<TaskCanceledException>(async () => { await task; });

            // and, the exception thrown by the stream is observed (this is an observing test)
        }

        private class TestStream : MemoryStream
        {
            private readonly ManualResetEventSlim _ev = new ManualResetEventSlim();

            public TestStream(byte[] bytes)
                : base(bytes)
            { }

            public void CompleteRead() => _ev.Set();

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                await Task.Yield();
                 _ev.Wait();
                throw new Exception("bang");
            }
        }
    }
}
