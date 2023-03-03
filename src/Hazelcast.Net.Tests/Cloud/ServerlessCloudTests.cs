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

using System;
using System.Threading.Tasks;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Cloud;

[Explicit("Requires Hazelcast Viridian Setup.")]
public class ServerlessCloudTests : ServerlessRemoteTestBase
{
    private string _hzVersion;

    [OneTimeSetUp]
    public void Setup()
    {
        _hzVersion = Environment.GetEnvironmentVariable("HZ_VERSION");

        if (string.IsNullOrEmpty(_hzVersion))
            throw new ArgumentNullException("HZ_VERSION", "HZ_VERSION must be set.");
    }

    [Test]
    public async Task TestCloud([Values] bool tlsEnabled, [Values] bool smartMode)
    {
        var cluster = await CreateCloudCluster(_hzVersion, tlsEnabled);

        var client = await CreateClientAsync(cluster, (options) => { options.Networking.SmartRouting = smartMode; });

        var map = await client.GetMapAsync<int, int>("myCloudMap");

        await map.PutAsync(1, 1);
        Assert.AreEqual(1, await map.GetAsync(1));

        await RcClient.StopCloudCluster(cluster.Id);

        await AssertEx.SucceedsEventually(() => Assert.AreEqual(ClientState.Disconnected, client.State), 10_000, 500);

        await RcClient.ResumeCloudClusterAsync(cluster.Id);

        await AssertEx.SucceedsEventually(() => Assert.AreEqual(ClientState.Connected, client.State), 10_000, 500);

        await map.PutAsync(2, 2);
        Assert.AreEqual(2, await map.GetAsync(2));
    }
}
