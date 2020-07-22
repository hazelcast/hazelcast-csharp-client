﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using NUnit.Framework;

namespace Hazelcast.Tests.NetStandard
{
    [TestFixture]
    public class TaskExtensionsTests
    {
        [Test]
        public void IsCompletedSuccessfully()
        {
            Assert.Throws<ArgumentNullException>(() => _ = ((Task) null).IsCompletedSuccessfully());

            var task = Task.CompletedTask;
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsCompletedSuccessfully, Is.True);

            task = Task.FromException(new Exception());
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsCompletedSuccessfully, Is.False);

            task = Task.FromCanceled(new CancellationToken(true));
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsCompletedSuccessfully, Is.False);
        }
    }
}
