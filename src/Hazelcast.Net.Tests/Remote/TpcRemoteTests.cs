// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote;

[TestFixture]
[Timeout(60_000)]
[Category("beta")]
internal class TpcRemoteTests : SingleMemberRemoteTestBase
{
    protected override string RcClusterConfiguration 
        => Resources.Cluster_Tpc;

    protected override HazelcastOptionsBuilder CreateHazelcastOptionsBuilder()
        => base.CreateHazelcastOptionsBuilder()
               .WithHConsoleLogger();

    [Test]
    public async Task CanPerformMapOperationsOnTpcCluster([Values] bool tpcEnabled)
    {
        HConsole.Configure(options => options.ConfigureDefaults(this));

        HConsole.WriteLine(this, "Begin");

        // create our own client so that its connection is captured by HConsole
        var options = CreateHazelcastOptions();

        // temp: this machine runs the Java server from master branch w/ latest patches, in debug mode
        /*
        options.Networking.Addresses.Clear();
        options.Networking.Addresses.Add("192.168.1.41:5701");
        options.ClusterName = "dev";
        */

        options.Networking.Tpc.Enabled = tpcEnabled;
        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        await using var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();

        await map.SetAsync("key", 42).CfAwait();
        await map.SetAsync("key", 43).CfAwait();

        var value = await map.GetAsync("key").CfAwait();
        Assert.AreEqual(43, value);

        var count = await map.GetSizeAsync().CfAwait();
        Assert.AreEqual(1, count);

        HConsole.WriteLine(this, "End");
    }
}