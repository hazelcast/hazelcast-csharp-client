// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class RingBufferDynamicConfigurationExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);


            // You can create and config a ring buffer on runtime.

            await client.DynamicOptions.ConfigureRingbufferAsync("configured-ringbuffer", options =>
            {
                // For other configuration options, please see https://docs.hazelcast.com/hazelcast/latest/data-structures/ringbuffer
                options.Capacity = 10;
                options.TimeToLiveSeconds = 3;
            });


            // get the distributed ring buffer from the cluster
            await using var ringBuffer = await client.GetRingBufferAsync<string>("ringbuffer-example");

            var writer = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    await ringBuffer.AddAsync("item " + i);
                }
            });

            var reader = Task.Run(async () =>
            {
                var sequence = await ringBuffer.GetHeadSequenceAsync();
                while (sequence < 100)
                {
                    var item = await ringBuffer.ReadOneAsync(sequence++);
                    Console.WriteLine("Reading value " + item);
                }
            });

            await Task.WhenAll(reader, writer);

            // destroy the ring buffer
            await client.DestroyAsync(ringBuffer);
        }
    }
}
