// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.DistributedObjects;
using Hazelcast.Examples.Models;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.WebSite
{
    public class IdentifiedEntryProcessor : IEntryProcessor<string>, IIdentifiedDataSerializable
    {
        public const int FactoryIdConst = 66; // Id of EntryProcessorDataSerializableFactory
        public const int ClassIdConst = 1; // corresponds to hazelcast-test.jar com.hazelcast.client.test.IdentifiedEntryProcessor

        private string _value;

        public IdentifiedEntryProcessor(string value)
        {
            _value = value;
        }

        public void ReadData(IObjectDataInput input)
        {
            _value = input.ReadString();
        }

        public void WriteData(IObjectDataOutput output)
        {
        }

        public int FactoryId => FactoryIdConst;

        public int ClassId => ClassIdConst;
    }

    // ReSharper disable once UnusedMember.Global
    public class EntryProcessorExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost

            options.Serialization.AddDataSerializableFactory(
                EntryProcessorDataSerializableFactory.FactoryId,
                new EntryProcessorDataSerializableFactory());

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // Get the Distributed Map from Cluster.
            await using var map = await client.GetMapAsync<string, string>("my-distributed-map");
            //Set the key-value pair
            await map.SetAsync("key", "value");
            // Run the IdentifiedEntryProcessor class on the Hazelcast Cluster Member holding the key called "key"
            await map.ExecuteAsync<string>(new IdentifiedEntryProcessor("duh"), "key");
            // Show that the IdentifiedEntryProcessor updated the value.
            Console.WriteLine("new value:" + await map.GetAsync("key"));
        }
    }
}
