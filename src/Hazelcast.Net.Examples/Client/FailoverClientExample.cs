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

namespace Hazelcast.Examples.Client
{
    /// <summary>
    /// The example introduces the initialization of a failover client.
    /// </summary>
    public class FailoverClientExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastFailoverOptionsBuilder()
                .With(p =>
                {
                    var clusterBlue = new HazelcastOptions();
                    clusterBlue.ClusterName = "blue";
                    clusterBlue.Networking.Addresses.Add("127.0.0.1:5701");
                    clusterBlue.Networking.ReconnectMode = Networking.ReconnectMode.ReconnectSync;
                    // Connection time out from a whole cluster (not a member).
                    clusterBlue.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 1_000;
                    //Subscribe to client state changes.
                    clusterBlue.AddSubscriber(s => s.StateChanged((sender, args) => Console.WriteLine($"Client state changed: {args.State}")));
                    //clusterBlue sets all options for the client.
                    p.Clients.Add(clusterBlue);

                    var clusterGreen = new HazelcastOptions();
                    clusterGreen.ClusterName = "green";
                    clusterGreen.Networking.Addresses.Add("127.0.0.2:5701");
                    clusterGreen.Networking.ReconnectMode = Networking.ReconnectMode.ReconnectSync;
                    //clusterGreen can only change network and authentication options
                    //other options must be same with first cluster, such as heartbeat, loadBalancer etc.
                    p.Clients.Add(clusterGreen);

                    p.TryCount = 2;
                })
                .With(args)
                .Build();

            // create a failover client
            await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(options); // disposed when method exits

            var map = await client.GetMapAsync<int, int>("myFailoverMap");

            for (int i = 0; i < 100; i++)
            {
                await map.PutAsync(i, i);
                Console.WriteLine($"Put: key={i}, val={i} on Cluster:{client.ClusterName}");
                //Plug off the clusterBlue to observe failover, client will change cluster to clusterGreen.
                await Task.Delay(1_000);
            }
        }
    }
}
