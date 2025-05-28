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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core;

[TestFixture]
public class AsyncReaderWriterLockTests
{
    [Test]
    public async Task Test()
    {
        var l = new AsyncReaderWriterLock();

        // can acquire multiple read locks
        var r0 = await l.ReadLockAsync();
        var r1 = await l.ReadLockAsync();
        var r2 = await l.ReadLockAsync();

        // read locks block write lock
        var writeLocking = l.WriteLockAsync();

        await Task.Delay(200);
        Assert.That(writeLocking.IsCompleted, Is.False);

        r0.Dispose();
        r1.Dispose();

        await Task.Delay(200);
        Assert.That(writeLocking.IsCompleted, Is.False);

        r2.Dispose();

        // no more read locks = can acquire write lock
        await Task.Delay(200);
        Assert.That(writeLocking.IsCompleted);
        var w0 = writeLocking.Result;

        // write lock blocks read locks
        var readLocking = l.ReadLockAsync();

        await Task.Delay(200);
        Assert.That(readLocking.IsCompleted, Is.False);

        w0.Dispose();

        // no more write lock = can acquire read locks
        await Task.Delay(200);
        Assert.That(readLocking.IsCompleted);
        var r3 = readLocking.Result;

        // can dispose
        var disposing = l.DisposeAsync();
        await Task.Delay(200);
        Assert.That(disposing.IsCompleted);

        // leaked locks don't throw
        r3.Dispose();
    }
}