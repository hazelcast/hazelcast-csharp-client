// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class CancellationTokenSourceExtensionsTests
    {
        [Test]
        public void LinkedWith1()
        {
            var cancellation1 = new CancellationTokenSource();
            var cancellation2 = new CancellationTokenSource();

            var cancellation = cancellation1.LinkedWith(cancellation2.Token);

            Assert.That(cancellation1.IsCancellationRequested, Is.False);
            Assert.That(cancellation2.IsCancellationRequested, Is.False);

            Assert.That(cancellation.IsCancellationRequested, Is.False);
            cancellation1.Cancel();
            Assert.That(cancellation.IsCancellationRequested, Is.True);

            Assert.That(cancellation1.IsCancellationRequested, Is.True);
            Assert.That(cancellation2.IsCancellationRequested, Is.False);
        }

        [Test]
        public void LinkedWith2()
        {
            var cancellation1 = new CancellationTokenSource();
            var cancellation2 = new CancellationTokenSource();

            var cancellation = cancellation1.LinkedWith(cancellation2.Token);

            Assert.That(cancellation1.IsCancellationRequested, Is.False);
            Assert.That(cancellation2.IsCancellationRequested, Is.False);

            Assert.That(cancellation.IsCancellationRequested, Is.False);
            cancellation2.Cancel();
            Assert.That(cancellation.IsCancellationRequested, Is.True);

            Assert.That(cancellation1.IsCancellationRequested, Is.False);
            Assert.That(cancellation2.IsCancellationRequested, Is.True);
        }

        [Test]
        public void ThrowIfCancellationRequested()
        {
            var cancellation = new CancellationTokenSource();

            cancellation.ThrowIfCancellationRequested();

            cancellation.Cancel();

            Assert.Throws<OperationCanceledException>(() => cancellation.ThrowIfCancellationRequested());
        }
    }
}
