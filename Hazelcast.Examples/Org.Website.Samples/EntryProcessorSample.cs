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
using Hazelcast.Client;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Examples.Org.Website.Samples
{
    public class IncEntryProcessor : IEntryProcessor, IIdentifiedDataSerializable
    {
        private const int ClassId = 1;
        private const int FactoryId = 66;

        public void ReadData(IObjectDataInput input)
        {
        }

        public void WriteData(IObjectDataOutput output)
        {
        }

        public int GetFactoryId()
        {
            return FactoryId;
        }

        public int GetId()
        {
            return ClassId;
        }
    }

    public class EntryProcessorSample
    {
        public static void Run(string[] args)
        {
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var hz = HazelcastClient.NewHazelcastClient();
            // Get the Distributed Map from Cluster.
            var map = hz.GetMap<string, int>("my-distributed-map");
            // Put the integer value of 0 into the Distributed Map
            map.Put("key", 0);
            // Run the IncEntryProcessor class on the Hazelcast Cluster Member holding the key called "key"
            map.ExecuteOnKey("key", new IncEntryProcessor());
            // Show that the IncEntryProcessor updated the value.
            Console.WriteLine("new value:" + map.Get("key"));
            // Shutdown this Hazelcast Client
            hz.Shutdown();
        }
    }
}