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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class SemaphoreAcquisitionTests
    {
        [Test]
        public async Task AcquireWithTaskCompleted()
        {
            var s = new SemaphoreSlim(1);

            var task = Task.Run(async () => await s.WaitAsync());
            await task;

            var a = SemaphoreAcquisition.Create(task, s);

            Assert.That(a.Acquired, Is.True);
            Assert.That((bool) a, Is.True);
            Assert.That(s.Wait(0), Is.False);

            a.Dispose();

            Assert.That(s.Wait(0), Is.True);
            s.Release();

            s.Dispose();
        }

        [Test]
        public async Task AcquireWithTaskNotCompleted()
        {
            var s = new SemaphoreSlim(0);

            var task = Task.Run(async () => await s.WaitAsync());

            var a = SemaphoreAcquisition.Create(task, s);

            Assert.That(a.Acquired, Is.False);
            Assert.That((bool)a, Is.False);

            a.Dispose();

            Assert.That(s.Wait(0), Is.False);

            s.Release();
            await task;

            s.Dispose();
        }

        [Test]
        public async Task AcquireWithTaskOfBoolTrue()
        {
            var s = new SemaphoreSlim(1);

            var task = Task.Run(async () => await s.WaitAsync(0));
            await task;

            var a = SemaphoreAcquisition.Create(task, s);

            Assert.That(a.Acquired, Is.True);
            Assert.That((bool) a, Is.True);
            Assert.That(s.Wait(0), Is.False);

            a.Dispose();

            Assert.That(s.Wait(0), Is.True);
            s.Release();

            s.Dispose();
        }

        [Test]
        public async Task AcquireWithTaskOfBoolFalse()
        {
            var s = new SemaphoreSlim(0);

            var task = Task.Run(async () => await s.WaitAsync(0));
            await task;

            var a = SemaphoreAcquisition.Create(task, s);

            Assert.That(a.Acquired, Is.False);
            Assert.That((bool) a, Is.False);
            Assert.That(s.Wait(0), Is.False);

            a.Dispose();

            Assert.That(s.Wait(0), Is.False);

            s.Dispose();
        }
    }
}
