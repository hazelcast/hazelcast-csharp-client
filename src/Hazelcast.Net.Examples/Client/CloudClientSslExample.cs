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

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class CloudClientSslExample
    {
        public static async Task Run()
        {
            static void Configure(HazelcastOptions configuration)
            {
                var cloud = configuration.Networking.Cloud;
                cloud.Enabled = true;
                cloud.DiscoveryToken = "DISCOVERY_TOKEN_HASH"; // copied from Cloud console

                var ssl = configuration.Networking.Ssl;
                ssl.Enabled = true;
                //ssl.ValidateCertificateChain = false;
                ssl.CertificatePath = "CLIENT_PFX_CERTIFICATE_PATH"; // downloaded from CLoud console
            }

            // create an Hazelcast client and connect to a Cloud server
            await using var client = new HazelcastClientFactory(HazelcastOptions.Build()).CreateClient(Configure);
            await client.ConnectAsync();

            // use a map
            await using var map = await client.GetMapAsync<string, string>("ssl-example");
            await map.AddOrUpdateAsync("key", "value");
            var value = await map.GetAsync("key");
            Console.WriteLine($"\"key\": \"{value}\"");
            await client.DestroyAsync(map);
        }
    }
}
