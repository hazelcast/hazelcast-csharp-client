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
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Transaction;

namespace Hazelcast.Examples.Transactions
{
    internal class TransactionBenchmark
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            //TODO fix here
            // config.SetClusterName("dev").SetClusterPassword("dev-pass");
            config.GetNetworkConfig().AddAddress("127.0.0.1:5701")
                .SetSmartRouting(false);

            var client = HazelcastClient.NewHazelcastClient(config);
            var map = client.GetMap<int, string>("test");
            var map2 = client.GetMap<int, string>("test3");
            map.Clear();
            map2.Clear();

            long totalDuration = 0;
            int count = 100;
            for (int i = 0; i < count; i++)
            {
                TransactionOptions options = new TransactionOptions()
                    .SetTransactionType(TransactionOptions.TransactionType.TwoPhase);
                ITransactionContext context = client.NewTransactionContext(options);

                var watch = System.Diagnostics.Stopwatch.StartNew();

                context.BeginTransaction();
                var tmap = context.GetMap<int, string>("test");
                var tmap2 = context.GetMap<int, string>("test2");

                tmap.Set(i, "value");
                tmap2.Set(i, "value");
                context.CommitTransaction();

                watch.Stop();
                totalDuration += watch.ElapsedMilliseconds;
            }
            Console.WriteLine((double)totalDuration / count);
        }
    }
}