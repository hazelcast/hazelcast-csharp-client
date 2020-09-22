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
using System.Diagnostics;
using System.Threading.Tasks;
using Hazelcast.Transactions;

namespace Hazelcast.Examples.Transactions
{
    // ReSharper disable once UnusedMember.Global
    public class BenchmarkExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = HazelcastClientFactory.CreateClient(options);
            await client.StartAsync();

            var map1 = await client.GetDictionaryAsync<int, string>("test1");
            var map2 = await client.GetDictionaryAsync<int, string>("test2");
            await map1.ClearAsync();
            await map2.ClearAsync();

            const int count = 100;
            var stopwatch = Stopwatch.StartNew();
            long totalDuration = 0;
            for (var i = 0; i < count; i++)
            {
                stopwatch.Restart();

                await using (var transactionContext = await client.BeginTransactionAsync(
                    new TransactionOptions { Type = TransactionOptions.TransactionType.TwoPhase }))
                {
                    var txMap1 = await transactionContext.GetDictionaryAsync<int, string>("test1");
                    var txMap2 = await transactionContext.GetDictionaryAsync<int, string>("test2");

                    await txMap1.SetAsync(i, "value");
                    await txMap2.SetAsync(i, "value");

                    await transactionContext.CommitAsync();
                }

                totalDuration += stopwatch.ElapsedMilliseconds;
            }

            Console.WriteLine((double) totalDuration / count);
        }
    }
}
