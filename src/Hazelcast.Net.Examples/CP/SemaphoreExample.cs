// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
    public class SemaphoreExample
    {
        public static async Task Main(string[] args)
        {

            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            // note that that server should be properly configured for CP with at least 3 members
            var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get a semaphore
            // for configuration, please visit https://docs.hazelcast.com/hazelcast/latest/cp-subsystem/configuration#semaphore-config
            var semaphoreInit = await client.CPSubsystem.GetSemaphore("my-semaphore");
            await semaphoreInit.InitializeAsync(2);

            async Task DoSomeWork(int task)
            {
                // Please visit https://hazelcast.github.io/hazelcast-csharp-client/latest/doc/locking.html#implicit-context
                // about async context.  
                AsyncContext.New();
                
                var semaphore = await client.CPSubsystem.GetSemaphore("my-semaphore");

                Console.WriteLine($"Task {task} acquiring...");
                var acquired = await semaphore.TryAcquireAsync(1,10_000);

                if (acquired)
                {
                    var availablePermit = await semaphore.GetAvailablePermitsAsync();
                    Console.WriteLine($"Task {task} acquired the permit. Doing some work. Available permit -> {availablePermit}");
                    
                    for (int i = 0; i < 5; i++)
                    {
                        Console.WriteLine($"Task {task} doing some work -> {i}");
                        await Task.Delay(500);
                    }

                    await semaphore.ReleaseAsync();
                    availablePermit = await semaphore.GetAvailablePermitsAsync();
                    Console.WriteLine($"Task {task} completed and released the permit. Available permit -> {availablePermit}");
                }
                else
                {
                    Console.WriteLine($"Task {task} failed to acquire the permit in 10sec.");
                }
            }
            
            // At most 2 tasks will work at the same time while 3rd one will wait to acquire a permit until timeout.
            var task1 = Task.Run(async () => { await DoSomeWork(1); });
            var task2 = Task.Run(async () => { await DoSomeWork(2); });
            var task3 = Task.Run(async () => { await DoSomeWork(3); });

            Console.WriteLine("Waiting for tasks...");
            await Task.WhenAll(task1, task2, task3);
            Console.WriteLine("Tasks completed.");
        }
    }
}
