// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Testing;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    [Timeout(30_000)]
    public class ClientTests : SingleMemberRemoteTestBase
    {
        [Test]
        public async Task ClientCanConnect()
        {
            // most basic test just to ensure that a client can connect

            //using var _ = HConsole.Capture(options => options
            //    .Set(x => x.SetLevel(1)));

            var client = await HazelcastClientFactory.StartNewClientAsync(CreateHazelcastOptions());

            await client.DisposeAsync();
        }

        [Test]
        public async Task ClientStartingClientWithConfig()
        {
            var clientStarting =  HazelcastClientFactory.GetNewStartingClient(CreateHazelcastOptions());
            await clientStarting.Task;
            await clientStarting.Client.DisposeAsync();
        }
        
        [Test]
        public async Task ClientStatringClientWithConfig2()
        {
            var o = CreateHazelcastOptions();
            var clientStarting =  HazelcastClientFactory.GetNewStartingClient(options=>
            {
                options.Networking.Addresses.Clear();
                options.Networking.Addresses.Add("127.0.0.1:5701");
                options.ClusterName = o.ClusterName;
            });
            await clientStarting.Task;
            await clientStarting.Client.DisposeAsync();
        }

        [Test]
        public async Task ClientCanConnectAsync()
        {
            // most basic test just to ensure that a client can connect

            //using var _ = HConsole.Capture(options => options
            //    .Set(x => x.SetLevel(1)));
            
            Assert.Throws<ArgumentNullException>(() => HazelcastClientFactory.GetNewStartingClient((HazelcastOptions)null));

            var clientStart = HazelcastClientFactory.GetNewStartingClient(CreateHazelcastOptions());
            var client = clientStart.Client;

            await clientStart.Task;

            await client.DisposeAsync();
        }

        [Test]
        [Category("enterprise")] // Failover is an Enterprise feature
        public async Task FailoverClientCanConnect()
        {
            // most basic test just to ensure that a failover client can connect

            //using var _ = HConsole.Capture(options => options
            //    .Set(x => x.SetLevel(1)));

            var client = await HazelcastClientFactory.StartNewFailoverClientAsync(CreateHazelcastFailoverOptions());

            await client.DisposeAsync();
        }

        [Test]
        [Category("enterprise")] // Failover is an Enterprise feature
        public async Task StartingFailoverClientCanConnect()
        {
            var startingClient = HazelcastClientFactory.GetNewStartingFailoverClient(CreateHazelcastFailoverOptions());
            await startingClient.Task;
            await startingClient.Client.DisposeAsync();
        }
        
        [Test]
        [Category("enterprise")] // Failover is an Enterprise feature
        public async Task StartingFailoverClientCanConnect2()
        {
            var o = CreateHazelcastFailoverOptions();
            var startingClient = HazelcastClientFactory.GetNewStartingFailoverClient(options =>
            {
                options.TryCount = o.TryCount;
                options.Clients.Add(o.Clients[0]);
            });
            await startingClient.Task;
            await startingClient.Client.DisposeAsync();
        }
        
        [Test]
        [Category("enterprise")] // Failover is an Enterprise feature
        public async Task FailoverClientCanConnect2()
        {
            // most basic test just to ensure that a failover client can connect

            //using var _ = HConsole.Capture(options => options
            //    .Set(x => x.SetLevel(1)));

            var client = await HazelcastClientFactory.StartNewFailoverClientAsync(opt =>
            {                
                opt.Clients.Add(CreateHazelcastOptions());
                opt.TryCount=1;
            });

            await client.DisposeAsync();
        }

        [Test]
        [Category("enterprise")] // Failover is an Enterprise feature
        public async Task FailoverClientCanConnectAsync()
        {
            // most basic test just to ensure that a failover client can connect

            //using var _ = HConsole.Capture(options => options
            //    .Set(x => x.SetLevel(1)));

            Assert.Throws<ArgumentNullException>(() => HazelcastClientFactory.GetNewStartingFailoverClient((HazelcastFailoverOptions)null));

            var clientStart = HazelcastClientFactory.GetNewStartingFailoverClient(CreateHazelcastFailoverOptions());
            var client = clientStart.Client;

            await clientStart.Task;

            await client.DisposeAsync();
        }

        private HazelcastFailoverOptions CreateHazelcastFailoverOptions()
        {
            var failoverOptions = new HazelcastFailoverOptionsBuilder().Build(builder =>
            {
                builder.AddHazelcastAndDefaults(null);
                builder.AddUserSecrets(GetType().Assembly, true);
            }, null, null, ConfigurationSecretsKey);

            failoverOptions.TryCount = 1;
            failoverOptions.Clients.Add(CreateHazelcastOptions());

            return failoverOptions;
        }
    }
}
