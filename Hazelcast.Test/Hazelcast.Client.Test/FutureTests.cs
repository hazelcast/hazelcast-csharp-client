// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Spi;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class FutureTests
    {
        static readonly object Result = new object();
        static readonly Exception Exception = new Exception();

        [Test]
        public void SyncFuture_Throws()
        {
            var future = new SyncFuture<object>();
            SetException(future);

            Assert.Throws(Is.EqualTo(Exception), () => future.WaitAndGet());
        }

        [Test]
        public void SyncFuture_SetsResult()
        {
            var future = new SyncFuture<object>();
            SetResult(future);

            Assert.AreEqual(Result, future.WaitAndGet());
        }

        [Test]
        public void SyncFuture_SetsNullResult()
        {
            var future = new SyncFuture<object>();
            SetNullResult(future);

            Assert.IsNull(future.WaitAndGet());
        }

        [Test]
        public async Task AsyncFuture_Throws()
        {
            var future = AsyncFuture<object>.Create(out var tcs);
            SetException(future);

            try
            {
                await tcs.Task;
                Assert.Fail("Should throw an exception");
            }
            catch (Exception e)
            {
                Assert.AreEqual(Exception, e);
            }
        }

        [Test]
        public async Task AsyncFuture_SetsResult()
        {
            var future = AsyncFuture<object>.Create(out var tcs);
            SetResult(future);

            Assert.AreEqual(Result, await tcs.Task);
        }

        [Test]
        public async Task AsyncFuture_SetsNullResult()
        {
            var future = AsyncFuture<object>.Create(out var tcs);
            SetNullResult(future);

            Assert.IsNull(await tcs.Task);
        }

        static void SetException(IFuture<object> future) => RunOnDifferentThread(() => future.SetException(Exception));
        static void SetResult(IFuture<object> future) => RunOnDifferentThread(() => future.SetResult(Result));
        static void SetNullResult(IFuture<object> future) => RunOnDifferentThread(() => future.SetResult(null));

        static void RunOnDifferentThread(Action action)
        {
            var thread = new Thread(() =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100)); // just to add some wait time
                action();
            })
            { IsBackground = true };
            thread.Start();
        }
    }
}