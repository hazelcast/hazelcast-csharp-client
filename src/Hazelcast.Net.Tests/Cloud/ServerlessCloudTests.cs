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
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Cloud;

// these tests require:
// that the RC runs with the following environment variables:
// - BASE_URL for instance https://api.dev.viridian.hazelcast.cloud
// - HZ_VERSION for instance 5.2.3
// - API_KEY ...
// - API_SECRET ...

// NOTE: do NOT rename that class, as the name is explicitly used in workflow
[Explicit("Requires Hazelcast Viridian Setup.")]
public class ServerlessCloudTests : CloudTestBase
{
    private string _hzVersion;

    private const int TestTimeout = 30 * 60 * 1_000; // 30 minutes
    private const int ClientTimeout = 10 * 60 * 1_000; // 10 minutes
    private const int MembersCount = 3; // clusters have 3 members
    private const int Polling = 500;

    [OneTimeSetUp]
    public void Setup()
    {
        _hzVersion = Environment.GetEnvironmentVariable("HZ_VERSION");

        if (string.IsNullOrWhiteSpace(_hzVersion))
        {
            Console.WriteLine("The cloud HZ_VERSION environment variable is not set, using default.");
            _hzVersion = "5.2.3"; // TODO: how can we figure out the latest stable?
        }
    }

    [Test]
    [Timeout(TestTimeout)]
    public async Task TestCloud([Values] bool tlsEnabled, [Values] bool smartMode)
    {
        HConsole.WriteLine(this, "Create cloud cluster");
        var cluster = await CreateCloudCluster(_hzVersion, tlsEnabled);

        HConsole.WriteLine(this, $"Create client (smart={(smartMode ? "true" : "false")})");
        await using var client = await CreateAndStartClientAsync(cluster, options => { options.Networking.SmartRouting = smartMode; });

        if (smartMode)
        {
            HConsole.WriteLine(this, "Wait for client to connect to all members");
            await AssertEx.SucceedsEventually(
                () => Assert.That(client.Members.Count(x => x.IsConnected), Is.EqualTo(MembersCount)),
                ClientTimeout, Polling);
        }

        HConsole.WriteLine(this, "Use client");
        await using var map = await client.GetMapAsync<int, int>("myCloudMap");
        await map.PutAsync(1, 1);
        Assert.AreEqual(1, await map.GetAsync(1));

        HConsole.WriteLine(this, "End");
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
    [Timeout(TestTimeout)]
    public async Task TestCloudWithResume([Values] bool tlsEnabled, [Values] bool smartMode)
    {
        using var _ = HConsole.Capture(x => x
            .Configure().SetLevel(0).EnableTimeStamp(origin: DateTime.Now) // default level
            .Configure<HConsoleLoggerProvider>().SetPrefix("LOG").SetMaxLevel() // always write the log output
            .Configure(this).SetMaxLevel().SetPrefix("TEST") // always write the test output
            .Configure<AsyncContext>().SetMinLevel() // do *not* write the AsyncContext verbose output
            .Configure<SocketConnectionBase>().SetIndent(1).SetLevel(0).SetPrefix("SOCKET"));

        HConsole.WriteLine(this, "Create cloud cluster");
        var cluster = await CreateCloudCluster(_hzVersion, tlsEnabled);

        HConsole.WriteLine(this, $"Create client (smart={(smartMode ? "true" : "false")})");
        await using var client = await CreateAndStartClientAsync(cluster, options => { options.Networking.SmartRouting = smartMode; });
        HConsole.WriteLine(this, "Client is connected");

        if (smartMode)
        {
            HConsole.WriteLine(this, "Wait for client to connect to all members");
            await AssertEx.SucceedsEventually(
                () => Assert.That(client.Members.Count(x => x.IsConnected), Is.EqualTo(MembersCount)),
            ClientTimeout, Polling);
            HConsole.WriteLine(this, "Smart client is connected to all members");
        }

        HConsole.WriteLine(this, "Use client");
        await using var map = await client.GetMapAsync<int, int>("myCloudMap");
        await map.PutAsync(1, 1);
        Assert.AreEqual(1, await map.GetAsync(1));

        HConsole.WriteLine(this, "Stop cluster");
        await RcClient.StopCloudClusterAsync(cluster.Id);
        HConsole.WriteLine(this, "Stopped cluster");
        await AssertEx.SucceedsEventually(
            () => Assert.That(client.State, Is.EqualTo(ClientState.Disconnected)),
            () => client.State == ClientState.Shutdown, // no point waiting
            ClientTimeout, Polling);
        HConsole.WriteLine(this, "Client is disconnected");

        HConsole.WriteLine(this, "Resume cluster");
        await RcClient.ResumeCloudClusterAsync(cluster.Id);
        HConsole.WriteLine(this, "Resumed cluster");
        await AssertEx.SucceedsEventually(
            () => Assert.That(client.State, Is.EqualTo(ClientState.Connected)),
            () => client.State == ClientState.Shutdown, // no point waiting
            ClientTimeout, Polling);
        HConsole.WriteLine(this, "Client is reconnected");

        HConsole.WriteLine(this, "Use client");
        await map.PutAsync(2, 2);
        Assert.AreEqual(2, await map.GetAsync(2));

        HConsole.WriteLine(this, "End");
    }
}
