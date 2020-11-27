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

namespace Hazelcast.Examples.WebSite
{
    // ReSharper disable once UnusedMember.Global
    public class RingBufferExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(BuildExampleOptions(args));

            await using var rb = await client.GetRingBufferAsync<long>("rb");

            // add two items into ring buffer
            await rb.AddAsync(100);
            await rb.AddAsync(200);

            // we start from the oldest item.
            // if you want to start from the next item, call rb.tailSequence()+1
            var sequence = await rb.GetHeadSequenceAsync();
            Console.WriteLine(await rb.ReadOneAsync(sequence));
            sequence += 1;
            Console.WriteLine(await rb.ReadOneAsync(sequence));

            await client.DestroyAsync(rb);
        }
    }
}
