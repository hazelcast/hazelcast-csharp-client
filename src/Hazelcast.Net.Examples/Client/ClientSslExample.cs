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
    public class ClientSslExample : ExampleBase
    {
        public static async Task Run(params string[] args)
        {
            var options = BuildExampleOptions(args);

            // server certificate will be validated by OS,
            // signed certificates will just work,
            // self-signed certificates should be registered and allowed
            options.Networking.Ssl.Enabled = true;

            // disable certificate validation
            //options.Networking.Ssl.ValidateCertificateChain = false;

            // validate the server certificate name
            //options.Networking.Ssl.ValidateCertificateName = true;
            //options.Networking.Ssl.CertificateName = "CERTIFICATE CN OR SAN VALUE HERE";

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartClientAsync(options);

            // use a map
            await using var map = await client.GetDictionaryAsync<string, string>("ssl-example");
            await map.SetAsync("key", "value");
            var value = await map.GetAsync("key");
            Console.WriteLine($"\"key\": \"{value}\"");
            await client.DestroyAsync(map);
        }
    }
}
