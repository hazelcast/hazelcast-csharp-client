// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Test
{
    public class ListenerApp
    {
        private static void Main2(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var clientConfig = new ClientConfig();
            clientConfig.GetNetworkConfig().AddAddress("127.0.0.1");
            var hc = HazelcastClient.NewHazelcastClient(clientConfig);
            var listener1 = new EntryAdapter<string, string>(
                @event => Console.WriteLine("ADD"),
                @event => Console.WriteLine("REMOVE"),
                @event => Console.WriteLine("UPDATE"),
                @event => Console.WriteLine("EVICTED"));

            var map = hc.GetMap<string, string>("default");
            var reg1 = map.AddEntryListener(listener1, false);
        }
    }
}