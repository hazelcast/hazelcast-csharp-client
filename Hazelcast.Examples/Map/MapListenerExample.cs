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
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Examples.Map
{
    internal class MapListenerExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<string, string>("listener-example");

            var cdown = new CountdownEvent(3);
            map.AddEntryListener(new EntryAdapter<string, string>
            {
                Added = e =>
                {
                    Console.WriteLine("Key '{0}' with value ' {1}' was added.", e.GetKey(), e.GetValue());
                    cdown.Signal();
                },
                Updated = e =>
                {
                    Console.WriteLine("Key '{0}' with value ' {1}' was added.", e.GetKey(), e.GetValue());
                    cdown.Signal();
                },
                Removed = e =>
                {
                    Console.WriteLine("Key '{0}' with value ' {1}' was removed.", e.GetKey(), e.GetOldValue());
                    cdown.Signal();
                }
            }, true);

            map.Put("key", "value");//first put
            map.Put("key", "valueNew");//makes update
            map.Remove("key");

            cdown.Wait();
            map.Destroy();
        }
    }
}