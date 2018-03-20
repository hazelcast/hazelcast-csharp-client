// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Examples.Org.Website.Samples
{
    public class ListSample
    {
        public static void Run(string[] args)
        {
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var hz = HazelcastClient.NewHazelcastClient();
            // Get the Distributed List from Cluster.
            var list = hz.GetList<string>("my-distributed-list");
            // Add elements to the list
            list.Add("item1");
            list.Add("item2");

            // Remove the first element
            Console.WriteLine("Removed: " + list.Remove(0));
            // There is only one element left
            Console.WriteLine("Current size is " + list.Size());
            // Clear the list
            list.Clear();
            // Shutdown this Hazelcast client
            hz.Shutdown();
        }
    }
}