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
using Hazelcast.Core;
using Hazelcast.Examples.Models;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.Map
{
    // ReSharper disable once UnusedMember.Global
    public class MapPortableExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // customize options for this example
            options.Serialization.AddPortableFactory(1, new ExamplePortableFactory());

            // note: this is another way to do it, which lazily creates the factory when and if needed
            /*
            options.Serialization.PortableFactories.Add(new FactoryOptions<IPortableFactory>
            {
                Id = 1,
                Creator = () => new ExamplePortableFactory()
            });
            */

            // create an Hazelcast client and connect to a server running on localhost
            await using var hz = new HazelcastClientFactory(options).CreateClient();
            await hz.OpenAsync().CAF();

            // get the distributed map from the cluster
            var map = await hz.GetMapAsync<int, Customer>("portable-example").CAF();

            // create an add a customer
            Console.WriteLine("Add customer 'first-customer'.");
            var customer = new Customer { Id = 1, LastOrder = DateTime.UtcNow, Name = "first-customer" };
            await map.AddOrReplaceAsync(customer.Id, customer).CAF();

            // retrieve customer
            var c = await map.GetAsync(customer.Id).CAF();
            Console.WriteLine($"Got customer '{c.Name}'.");

            // destroy the map
            await hz.DestroyAsync(map).CAF();
        }
    }
}
