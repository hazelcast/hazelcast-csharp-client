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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Events;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    internal class ClientLifecycleExample
    {
        public static async Task Run(params string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            var options = HazelcastOptions.Build(args);
            var hz1 = await HazelcastClientFactory.StartClientAsync(options);

            var connected = new SemaphoreSlim(0);

            // create another Hazelcast client and connect to a server running on localhost
            options.AddSubscriber(on => on
                .ClientStateChanged((c, eventArgs) =>
                {
                    Console.WriteLine($"State: {eventArgs.State}");
                    if (eventArgs.State == ClientLifecycleState.Connected)
                        connected.Release();
                }));
            var hz2 = await HazelcastClientFactory.StartClientAsync(options);

            // wait for the event
            await connected.WaitAsync();

            // terminate the clients
            await hz1.DisposeAsync();
            await hz2.DisposeAsync();
        }
    }
}
