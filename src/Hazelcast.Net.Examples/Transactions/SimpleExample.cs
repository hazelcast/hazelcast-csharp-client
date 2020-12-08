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
using System.Threading.Tasks;
using Hazelcast.Transactions;

namespace Hazelcast.Examples.Transactions
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleExample : ExampleBase
    {
        public async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // begin a transaction
            await using (var transactionContext = await client.BeginTransactionAsync(
                new TransactionOptions { Type = TransactionOptions.TransactionType.OnePhase }))
            {
                // perform operations with the transaction
                try
                {
                    var transactionMap = await transactionContext.GetMapAsync<string, string>("txn-map");
                    await transactionMap.PutIfAbsentAsync("key", "value");
                    await transactionContext.CommitAsync();
                }
                catch
                {
                    await transactionContext.RollbackAsync();
                }
            }

            // verify that value is available outside the transaction
            var map = await client.GetMapAsync<string, string>("txn-map");
            Console.WriteLine();
            Console.WriteLine("After transaction:");
            Console.WriteLine("map[\"key\"]: " + await map.GetAsync("key"));

            // again
            await using (var transactionContext = await client.BeginTransactionAsync(
                new TransactionOptions { Type = TransactionOptions.TransactionType.OnePhase }))
            {
                // perform operations with the transaction
                try
                {
                    var transactionMap = await transactionContext.GetMapAsync<string, string>("txn-map");
                    await transactionMap.ReplaceAsync("key", "value", "other");
                    Console.WriteLine();
                    Console.WriteLine("In transaction:");
                    Console.WriteLine("txMap[\"key\"]: " + await transactionMap.GetAsync("key"));
                    Console.WriteLine("map[\"key\"]: " + await map.GetAsync("key"));

                    throw new Exception("boom"); // will roll back
                }
                catch
                {
                    await transactionContext.RollbackAsync();
                }
            }

            Console.WriteLine();
            Console.WriteLine("After transaction:");
            Console.WriteLine("map[\"key\"]: " + await map.GetAsync("key"));

            // again
            await using (var transactionContext = await client.BeginTransactionAsync(
                new TransactionOptions { Type = TransactionOptions.TransactionType.OnePhase }))
            {
                // perform operations with the transaction
                var transactionMap = await transactionContext.GetMapAsync<string, string>("txn-map");
                await transactionMap.ReplaceAsync("key", "value", "other");
                Console.WriteLine();
                Console.WriteLine("In transaction:");
                Console.WriteLine("txMap[\"key\"]: " + await transactionMap.GetAsync("key"));
                Console.WriteLine("map[\"key\"]: " + await map.GetAsync("key"));

                // neither committed nor rolled back...
                // will implicitly roll back
            }

            Console.WriteLine();
            Console.WriteLine("After transaction:");
            Console.WriteLine("map[\"key\"]: " + await map.GetAsync("key"));

            Console.WriteLine();

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
