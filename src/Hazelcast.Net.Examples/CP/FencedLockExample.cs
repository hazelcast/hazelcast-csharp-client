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

// ReSharper disable LocalizableElement

using System;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Examples.CP
{
    public class FencedLockExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            // note that that server should be properly configured for CP with at least 3 members
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get a fenced lock
            var lockA = await client.CPSubsystem.GetLockAsync("lock-a");

            // get a context
            var context1 = new LockContext();

            // lock for context1 (count becomes 1)
            await lockA.LockAsync(context1);

            // re-enter the lock for context 1 (count becomes 2)
            await lockA.LockAsync(context1);

            // get another context
            var context2 = new LockContext();

            // cannot lock for context2
            if (await lockA.TryLockAsync(context2)) throw new Exception("Should be false?");

            // release the lock once (count becomes 1 = still locked)
            await lockA.UnlockAsync(context1);

            // cannot lock for context2
            if (await lockA.TryLockAsync(context2)) throw new Exception("Should be false?");

            // release the lock again (count becomes 0 = unlocked)
            await lockA.UnlockAsync(context1);

            // now can lock for context2
            if (!(await lockA.TryLockAsync(context2))) throw new Exception("Should be true?");

            // cleanup
            await lockA.UnlockAsync(context2);
            await lockA.DestroyAsync();
        }
    }
}
