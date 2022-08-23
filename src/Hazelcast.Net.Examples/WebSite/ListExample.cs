﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Examples.WebSite
{
    // ReSharper disable once UnusedMember.Global
    public class ListExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // Get the Distributed List from Cluster.
            await using var list = await client.GetListAsync<string>("my-distributed-list");

            // Add elements to the list
            await list.AddAsync("item1");
            await list.AddAsync("item2");

            // Remove the first element
            Console.WriteLine("Removed: " + await list.RemoveAsync(0));
            // There is only one element left
            Console.WriteLine("Current size is " + await list.GetSizeAsync());
            // Clear the list
            await list.ClearAsync();

            await client.DestroyAsync(list);
        }
    }
}
