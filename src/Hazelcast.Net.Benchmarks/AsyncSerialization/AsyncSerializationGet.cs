// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;

namespace Hazelcast.Benchmarks.AsyncSerialization
{
    public class AsyncSerializationGet
    {
        private IHMap<int, int> _map;

        [GlobalSetup]
        public void SetupNonAsync()
        {
            Setup().Wait(); // somehow Benchmark.NET does not like the async Setup?
        }

        public async Task Setup()
        {
            var options = new HazelcastOptionsBuilder()
                .Build();
            var client = await HazelcastClientFactory.StartNewClientAsync(options).CfAwait();
            _map = await client.GetMapAsync<int, int>("map").CfAwait();

            for (var i = 0; i < 10; i++)
            {
                await _map.PutAsync(i % 10, i % 10).CfAwait();
            }
        }

        [Benchmark(Baseline = true)]
        public async Task Get()
        {
            for (var i = 0; i < 100; i++)
            {
                var value = await _map.GetAsync(i % 10).CfAwait();
            }
        }

        [Benchmark]
        public async Task Get2()
        {
            for (var i = 0; i < 100; i++)
            {
                var value = await _map.GetAsync2(i % 10).CfAwait();
            }
        }
    }
}
