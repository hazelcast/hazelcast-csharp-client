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
using System.Threading.Tasks;

namespace Hazelcast.Examples.Security
{
    // ReSharper disable once UnusedMember.Global
    public class ClientSslMutualAuthExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // server certificate will be validated by OS,
            // signed certificates will just work,
            // self-signed certificates should be registered and allowed
            // otherwise;
            // disable certificate validation
            //options.Networking.Ssl.ValidateCertificateChain = false;

            // disable validation of the server certificate name
            //options.Networking.Ssl.ValidateCertificateName = false;
            
            options.Networking.Ssl.Enabled = true;

            // providing a client pfx certificate will enable mutual authentication
            // if the server is also configured for mutual authentication.
            options.Networking.Ssl.CertificatePath = "CLIENT_PFX_CERTIFICATE_PATH";
            options.Networking.Ssl.CertificatePassword = "CERTIFICATE PASSWORD HERE";
            
            // create an Hazelcast client and connect to a server running on localhost
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
