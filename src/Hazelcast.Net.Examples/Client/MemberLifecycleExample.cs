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

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class MemberLifecycleExample : ExampleBase
    {
        public static async Task Run(params string[] args)
        {
            var memberAdded = new SemaphoreSlim(0);

            var options = BuildExampleOptions(args, configureOptions: (configuration, options) =>
            {
                options.AddSubscriber(on => on
                    .MemberAdded((c, args) =>
                    {
                        Console.WriteLine($"Added member: {args.Member.Id}");
                        memberAdded.Release();
                    }));
            });

            // create an Hazelcast client and connect to a server running on localhost
            var hz = await HazelcastClientFactory.StartClientAsync(options);

            // wait for the event
            await memberAdded.WaitAsync();

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}
