// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class CloudClientExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .With(config =>
                {
                    // Get Started: https://docs.hazelcast.com/cloud/get-started
                    // Your Viridian cluster name.
                    config.ClusterName = "YOUR_CLUSTER_NAME";
                    // Your discovery token and url to connect Viridian cluster.
                    config.Networking.Cloud.DiscoveryToken = "YOUR_CLUSTER_DISCOVERY_TOKEN";
                    // Enable metrics to see on Management Center.
                    config.Metrics.Enabled = true;
                    // Configure SSL.
                    config.Networking.Ssl.Enabled = true;
                    config.Networking.Ssl.ValidateCertificateChain = false;
                    config.Networking.Ssl.Protocol = SslProtocols.Tls12;
                    config.Networking.Ssl.CertificatePath = "client.pfx";
                    config.Networking.Ssl.CertificatePassword = "YOUR_SSL_PASSWORD";
                })
                .WithConsoleLogger()
                .Build();
            
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // use a map
            await using var map = await client.GetMapAsync<string, string>("ssl-example");
            await map.SetAsync("key", "value");
            var value = await map.GetAsync("key");
            Console.WriteLine($"\"key\": \"{value}\"");
            await client.DestroyAsync(map);
        }
    }
}
