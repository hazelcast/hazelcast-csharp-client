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
using Hazelcast.Client;
using Hazelcast.Cluster;
using Hazelcast.Configuration;
using Hazelcast.Events;

namespace Hazelcast.Examples.Collections
{
    internal class CollectionListenerExample
    {
        private static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var list = client.GetList<string>("collection-listener-example");
            var cdown = new CountdownEvent(3);
            list.AddItemListener(new ItemListener<string>
            {
                OnItemAdded = e =>
                {
                    Console.WriteLine("Item added: " + e.GetItem());
                    cdown.Signal();
                },
                OnItemRemoved = e =>
                {
                    Console.WriteLine("Item removed: " + e.GetItem());
                    cdown.Signal();
                }
            }, true);

            list.Add("item1");
            list.Add("item2");
            list.Remove("item1");

            cdown.Wait();
            list.Destroy();
            client.Shutdown();
        }
    }
}
