// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    public class AtomicReferenceExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create a Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the CP AtomicReference from the cluster
            await using var aref = await client.CPSubsystem.GetAtomicReferenceAsync<string>($"atomicref-example-{Guid.NewGuid()}");
            Console.WriteLine($"Initial value: {await aref.GetAsync()}, is default: {await aref.IsNullAsync()}");

            await aref.SetAsync(RandomString());
            Console.WriteLine($"Value after set: {await aref.GetAsync()}, is default: {await aref.IsNullAsync()}");

            var previous = await aref.GetAndSetAsync(RandomString());
            Console.WriteLine($"Value after get-and-set: {await aref.GetAsync()}, previous: {previous}");

            previous = await aref.GetAsync();
            await aref.CompareAndSetAsync(previous, RandomString());
            Console.WriteLine($"Value after compare-and-set: {await aref.GetAsync()}, previous: {previous}");

            await aref.ClearAsync();
            Console.WriteLine($"Value after clear: {await aref.GetAsync()}, is default: {await aref.IsNullAsync()}");

            // destroy the AtomicReference
            await aref.DestroyAsync();
        }

        private static string RandomString() => Guid.NewGuid().ToString().Substring(0, 8);
    }
}
