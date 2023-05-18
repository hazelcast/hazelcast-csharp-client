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
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Cloud;

// NOTE: do NOT rename that class, as the name is explicitly used in workflow
// [Explicit("Requires Hazelcast Viridian Setup.")]
public class ServerlessCloudTests : CloudTestBase
{
    private string _hzVersion;

    [OneTimeSetUp]
    public void Setup()
    {
        _hzVersion = Environment.GetEnvironmentVariable("HZ_VERSION");

        if (string.IsNullOrWhiteSpace(_hzVersion))
            throw new ArgumentException("HZ_VERSION", "The cloud HZ_VERSION environment variable is not set.");
    }

    [Test]
    [Timeout(10 * 60 * 1_000)]
    public async Task TestCloud([Values] bool tlsEnabled, [Values] bool smartMode)
    {
        var cluster = await CreateCloudCluster(_hzVersion, tlsEnabled);
        var client = await CreateAndStartClientAsync(cluster, options => { options.Networking.SmartRouting = smartMode; });

        var map = await client.GetMapAsync<int, int>("myCloudMap");
        await map.PutAsync(1, 1);
        Assert.AreEqual(1, await map.GetAsync(1));
    }

    [Test]
    public async Task RcPathTest()
    {
        // verifies that we can get the current filesystem path of the RC process
        // (we need it in order to figure out where the RC saves the cloud certificates)
        var path = await RcClient.GetRcPathAsync();
        Assert.That(path, Is.Not.Null);
    }

    [Test]
    [Timeout(20 * 60 * 1_000)]
    public async Task TestCloudWithResume([Values] bool tlsEnabled, [Values] bool smartMode)
    {
        using var _ = HConsole.Capture(x => x
            .Configure().SetLevel(0).EnableTimeStamp(origin: DateTime.Now) // default level
            .Configure<HConsoleLoggerProvider>().SetPrefix("LOG").SetMaxLevel() // always write the log output
            .Configure(this).SetMaxLevel().SetPrefix("TEST") // always write the test output
            .Configure<AsyncContext>().SetMinLevel() // do *not* write the AsyncContext verbose output
            .Configure<SocketConnectionBase>().SetIndent(1).SetLevel(0).SetPrefix("SOCKET"));

        var cluster = await CreateCloudCluster(_hzVersion, tlsEnabled);
        var client = await CreateAndStartClientAsync(cluster, options => { options.Networking.SmartRouting = smartMode; });

        var map = await client.GetMapAsync<int, int>("myCloudMap");
        await map.PutAsync(1, 1);
        Assert.AreEqual(1, await map.GetAsync(1));

        await RcClient.StopCloudClusterAsync(cluster.Id);
        await AssertEx.SucceedsEventually(() => Assert.AreEqual(ClientState.Disconnected, client.State),
            10 * 60 * 1_000, 500);

        await RcClient.ResumeCloudClusterAsync(cluster.Id);
        await AssertEx.SucceedsEventually(() => Assert.AreEqual(ClientState.Connected, client.State), 
            10 *60 * 1_000, 500);

        await map.PutAsync(2, 2);
        Assert.AreEqual(2, await map.GetAsync(2));
    }
}
