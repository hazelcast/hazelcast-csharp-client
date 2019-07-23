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
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Primitives
{
    internal class AtomicLongExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var atomicLong = client.GetAtomicLong("atomic-long-exmaple");

            Action increment = () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    atomicLong.IncrementAndGet();
                }
            };
            var task1 = Task.Factory.StartNew(increment);
            var task2 = Task.Factory.StartNew(increment);

            Task.WaitAll(task1, task2);
            Console.WriteLine("Final value: " + atomicLong.Get());
            atomicLong.Destroy();
        }
    }
}