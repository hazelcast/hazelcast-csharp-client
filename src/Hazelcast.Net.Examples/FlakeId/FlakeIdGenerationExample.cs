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

using System;
using System.Threading.Tasks;
using Hazelcast.FlakeId;

// ReSharper disable LocalizableElement

namespace Hazelcast.Examples.FlakeId
{
    public class FlakeIdGenerationExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(opts =>
                {
                    // Configure all generators with specific prefix
                    opts.FlakeIdGenerators["flakeid-gen-*"] = new FlakeIdGeneratorOptions
                    {
                        PrefetchCount = 10,
                        PrefetchValidityPeriod = TimeSpan.FromSeconds(5)
                    };
                })
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create a Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the Flake Id Generator from the cluster
            var generator = await client.GetFlakeIdGeneratorAsync("flakeid-gen-1");

            // get first batch with ids
            var (id1, id2, id3) = (await generator.GetNewIdAsync(), await generator.GetNewIdAsync(), await generator.GetNewIdAsync());
            Console.WriteLine($"Generated ids: {id1}, {id2}, {id3}");

            // wait for batch to become outdated
            var validityPeriod = client.Options.GetFlakeIdGeneratorOptions(generator.Name).PrefetchValidityPeriod;
            Console.WriteLine($"Waiting for {validityPeriod}");
            await Task.Delay(validityPeriod);

            // next batch will be fetched here
            var (id4, id5, id6) = (await generator.GetNewIdAsync(), await generator.GetNewIdAsync(), await generator.GetNewIdAsync());
            Console.WriteLine($"Generated ids: {id4}, {id5}, {id6}");
        }
    }
}
