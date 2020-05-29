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

namespace Hazelcast.Examples.Cloud
{
    // ReSharper disable once UnusedMember.Global
    public class HazelcastCloudWithSslExample
    {
        public static async Task Run()
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            static void Configure(HazelcastConfiguration configuration)
            {
                var cloud = configuration.Networking.Cloud;
                cloud.IsEnabled = true;
                cloud.DiscoveryToken = "DISCOVERY_TOKEN_HASH"; // copied from Cloud console

                var ssl = configuration.Networking.Ssl;
                ssl.IsEnabled = true;
                //ssl.ValidateCertificateChain = false;
                ssl.CertificatePath = "CLIENT_PFX_CERTIFICATE_PATH"; // downloaded from CLoud console
            }

            // create an Hazelcast client and connect to a Cloud server
            var hz = new HazelcastClientFactory().CreateClient(Configure);
            await hz.OpenAsync();

            // use a map
            var map = await hz.GetMapAsync<string, string>("ssl-example");
            await map.AddOrReplaceAsync("key", "value");
            var value = await map.GetAsync("key");
            Console.WriteLine($"\"key\": \"{value}\"");
            map.Destroy();

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}
