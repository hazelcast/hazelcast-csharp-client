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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core;

[TestFixture]
public class ParallelRunnerTests
{
    [Test]
    public async Task RunnerThrowsExceptions()
    {
        var yielded = 0;

        IEnumerable<Task> SourceTasks()
        {
            yielded++;
            yield return RunAsync(false);

            yielded++;
            yield return RunAsync(true);

            yielded++;
            yield return RunAsync(false);

            yielded++;
            yield return RunAsync(false);
        }

        var e = await AssertEx.ThrowsAsync<Exception>(async () => await ParallelRunner.Run(SourceTasks(), new ParallelRunner.Options()));

        Assert.That(yielded, Is.EqualTo(4));
    }

    private static async Task RunAsync(bool andThrow)
    {
        await Task.Yield();
        if (andThrow) throw new Exception("bang");
    }
}