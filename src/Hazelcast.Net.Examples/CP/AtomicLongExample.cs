// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

// ReSharper disable LocalizableElement

namespace Hazelcast.Examples.CP
{
    public class AtomicLongExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the CP AtomicLong from the cluster
            await using var along = await client.CPSubsystem.GetAtomicLongAsync($"atomiclong-example-{Guid.NewGuid()}");
            Console.WriteLine($"Initial value: {await along.GetAsync()}");

            await along.SetAsync(10);
            Console.WriteLine($"Value after set: {await along.GetAsync()}");

            var previous = await along.IncrementAndGetAsync();
            Console.WriteLine($"Value after increment: {await along.GetAsync()}, previous: {previous}");

            previous = await along.DecrementAndGetAsync();
            Console.WriteLine($"Value after decrement: {await along.GetAsync()}, previous: {previous}");

            previous = await along.AddAndGetAsync(5);
            Console.WriteLine($"Value after add: {await along.GetAsync()}, previous: {previous}");

            previous = await along.GetAndSetAsync(100);
            
            Console.WriteLine($"Value after get&set: {await along.GetAsync()}, previous: {previous}");

            // destroy the AtomicLong
            await along.DestroyAsync();
        }
    }
}
