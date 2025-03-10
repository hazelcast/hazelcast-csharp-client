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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Protocol.TempCodecs;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Hazelcast.Testing.Networking;
using Hazelcast.Testing.TestServer;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote;

[TestFixture]
[Category("beta")]
[Explicit("TPC Requires to be promoted to prod.")]
internal class TpcLocalTests
{
    [Test]
    [Timeout(30_000)]
    public async Task CanConnect()
    {
        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        var tpcConnect = new ManualResetEventAsync(false);
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens, tpcConnect);

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        await using var client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));

        // but TPC connections are still pending
        for (var i = 0; i < tpcServerCount; i++) Assert.That(clientIds[i] == Guid.Empty);

        // unlock TPC connections: after a while, all TPC connections have been established
        tpcConnect.Set();
        await AssertEx.SucceedsEventually(() =>
        {
            for (var i = 0; i < tpcServerCount; i++)
            {
                Assert.That(clientIds[i], Is.EqualTo(client.Id));
                Assert.That(tokens[i], Is.EqualTo(tpcToken));
            }
        }, 10_000, 500);

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    [Test]
    [Timeout(30_000)]
    public async Task CanDisconnect()
    {
        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens);

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        var client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));

        // after a while, all TPC connections have been established
        await AssertEx.SucceedsEventually(() =>
        {
            for (var i = 0; i < tpcServerCount; i++)
            {
                Assert.That(clientIds[i], Is.EqualTo(client.Id));
                Assert.That(tokens[i], Is.EqualTo(tpcToken));
            }
        }, 10_000, 500);

        foreach (var server in tpcServers)
            Assert.That(server.ConnectionCount, Is.EqualTo(1));

        // stop the client
        await client.DisposeAsync();

        // eventually, all TPC servers are disconnected
        await AssertEx.SucceedsEventually(() =>
        {
            foreach (var server in tpcServers)
                Assert.That(server.ConnectionCount, Is.Zero);
        }, 10_000, 1000);

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    [Test]
    [Timeout(30_000)]
    public async Task IfOneTpcGoesDown()
    {
        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens);

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        await using var client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));

        // after a while, all TPC connections have been established
        await AssertEx.SucceedsEventually(() =>
        {
            for (var i = 0; i < tpcServerCount; i++)
            {
                Assert.That(clientIds[i], Is.EqualTo(client.Id));
                Assert.That(tokens[i], Is.EqualTo(tpcToken));
            }
        }, 10_000, 500);

        // killing one TPC connection...
        HConsole.WriteLine(this, "STOP TPC0");
        await tpcServers[0].StopAsync();

        // ... eventually kills the entire client
        await AssertEx.SucceedsEventually(() => Assert.That(client.IsActive, Is.False), 10_000, 1000);

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 1; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    [Test]
    [Timeout(30_000)]
    public async Task IfOneTpcThrowsEventually([Values] bool tpcRequired)
    {
        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens);

        // that one will *not* connect, but throw
        var tpcConnect = new ManualResetEventAsync(false);
        tpcServers[0].Handle(TpcClientChannelAuthenticationServerCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            _ = TpcClientChannelAuthenticationServerCodec.DecodeRequest(request.Message);
            await tpcConnect.WaitAsync(CancellationToken.None);
            await request.ErrorAsync(RemoteError.UnsupportedOperation, "go away");
        });

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        clientOptions.Networking.Tpc.Required = tpcRequired;
        await using var client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));

        // client *is* active
        Assert.That(client.IsActive, Is.True);
        HConsole.WriteLine(this, "STARTED CLIENT");

        // but one TPC will throw...
        tpcConnect.Set();

        if (tpcRequired)
        {
            // ... eventually killing the entire client
            await AssertEx.SucceedsEventually(() => Assert.That(client.IsActive, Is.False), 10_000, 1000);

            // ... and all connections
            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(classicServer.ConnectionCount, Is.Zero, "classic is still connected");
                for (var i = 0; i < tpcServerCount; i++) Assert.That(tpcServers[i].ConnectionCount, Is.Zero, $"tpc {i} is still connected");
            }, 10_000, 500);
        }
        else
        {
            // ... and we'll fall back to classic
            await Task.Delay(20_000);
            Assert.That(client.IsActive);
            Assert.That(classicServer.ConnectionCount, Is.Not.Zero, "classic is not connected");
            for (var i = 0; i < tpcServerCount; i++) Assert.That(tpcServers[i].ConnectionCount, Is.Zero, $"tpc {i} is still connected");
        }

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    [Test]
    [Timeout(30_000)]
    public async Task IfOneTpcThrowsImmediately([Values] bool tpcRequired)
    {
        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens);

        // that one will *not* connect, but throw
        tpcServers[0].Handle(TpcClientChannelAuthenticationServerCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            _ = TpcClientChannelAuthenticationServerCodec.DecodeRequest(request.Message);
            await request.ErrorAsync(RemoteError.UnsupportedOperation, "go away");
        });

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        clientOptions.Networking.Tpc.Required = tpcRequired;

        IHazelcastClient client = null;
        if (tpcRequired)
        {
            // but one TPC will throw even before we get the members list, so the client never really connects
            await AssertEx.ThrowsAsync<ConnectionException>(async () =>
            {
                client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
            });
        }
        else
        {
            // but one TPC will throw even before we get the member list, never mind
            client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
            Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));
        }

        if (tpcRequired)
        {
            // all connections down
            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(classicServer.ConnectionCount, Is.Zero, "classic is still connected");
                for (var i = 0; i < tpcServerCount; i++) Assert.That(tpcServers[i].ConnectionCount, Is.Zero, $"tpc {i} is still connected");
            }, 10_000, 500);
        }
        else
        {
            // client *is* active
            Assert.That(client.IsActive, Is.True);
            HConsole.WriteLine(this, "STARTED CLIENT");

            // running on classic
            await Task.Delay(20_000);
            Assert.That(client.IsActive);
            Assert.That(classicServer.ConnectionCount, Is.Not.Zero, "classic is not connected");
            for (var i = 0; i < tpcServerCount; i++) Assert.That(tpcServers[i].ConnectionCount, Is.Zero, $"tpc {i} is still connected");
        }

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    [Test]
    [Timeout(30_000)]
    public async Task IfOneTpcTimesOut([Values] bool tpcRequired)
    {
        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens);

        // that one will *not* connect, but throw
        var tpcConnect = new ManualResetEventAsync(false);
        tpcServers[0].Handle(TpcClientChannelAuthenticationServerCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            _ = TpcClientChannelAuthenticationServerCodec.DecodeRequest(request.Message);
            await tpcConnect.WaitAsync(CancellationToken.None);
            await request.ErrorAsync(RemoteError.UnsupportedOperation, "go away");
        });

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        clientOptions.Networking.Tpc.Required = tpcRequired;
        await using var client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));

        // client *is* active
        Assert.That(client.IsActive, Is.True);
        HConsole.WriteLine(this, "STARTED CLIENT");

        // but one TPC will never connect, causing a timeout ...

        if (tpcRequired)
        {
            // ... eventually killing the entire client
            await AssertEx.SucceedsEventually(() => Assert.That(client.IsActive, Is.False), 20_000, 1000);

            // ... and all connections
            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(classicServer.ConnectionCount, Is.Zero, "classic is still connected");
                for (var i = 0; i < tpcServerCount; i++) Assert.That(tpcServers[i].ConnectionCount, Is.Zero, $"tpc {i} is still connected");
            }, 10_000, 500);
        }
        else
        {
            // ... and we'll fall back to classic
            await Task.Delay(20_000);
            Assert.That(client.IsActive);
            Assert.That(classicServer.ConnectionCount, Is.Not.Zero, "classic is not connected");
            for (var i = 0; i < tpcServerCount; i++) Assert.That(tpcServers[i].ConnectionCount, Is.Zero, $"tpc {i} is still connected");
        }

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    [Test]
    [Timeout(30_000)]
    public async Task SendsRequestsToClassicOrTpc()
    {
        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens);

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        await using var client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));

        // will receive the partitions event
        await AssertEx.SucceedsEventually(() =>
        {
            Assert.That(((HazelcastClient)client).Cluster.State.Partitioner.Count, Is.EqualTo(3));
        }, 10_000, 500);

        // all TPC channels eventually connect
        await AssertEx.SucceedsEventually(() =>
        {
            for (var i = 0; i < tpcServerCount; i++) Assert.That(clientIds[i] != Guid.Empty);
        }, 10_000, 500);

        // a non-partitioned request goes to classic server
        classicServer.Handle(ClientCreateProxyCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            await request.RespondAsync(ClientCreateProxyServerCodec.EncodeResponse());
        });
        var map = await client.GetMapAsync<Thing, int>("map");

        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(3));
        for (var i = 0; i < tpcServerCount; i++) Assert.That(tpcServers[i].State.HandledMessages.Count, Is.EqualTo(1));

        // a partitioned request *should* go to a TPC server
        classicServer.Handle(MapSetCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            await request.RespondAsync(MapSetServerCodec.EncodeResponse());
        });
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i].Handle(MapSetCodec.RequestMessageType, async request =>
            {
                request.State.HandledMessages.Add(request.Message.MessageType);
                await request.RespondAsync(MapSetServerCodec.EncodeResponse());
            });

        // we're going to use Thing keys in order to force the partition key to what we want
        // the Thing class returns its Value (must be > 0) as partition hash, which is then % count -> tpc
        // sending Thing object will send schemas, ignore
        classicServer.Handle(ClientSendSchemaCodec.RequestMessageType, async request =>
        {
            await request.RespondAsync(ClientSendSchemaServerCodec.EncodeResponse(new HashSet<Guid> { request.Server.MemberId }));
        });

        var expected = new int[tpcServerCount];
        for (var i = 0; i < tpcServerCount; i++) expected[i] = 1;

        await map.SetAsync(new Thing { Value = 1 }, 1);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(3));
        expected[1] = 2;
        for (var i = 0; i < tpcServerCount; i++)
            Assert.That(tpcServers[i].State.HandledMessages.Count, Is.EqualTo(expected[i]), $"Server {i} count={tpcServers[i].State.HandledMessages.Count} expected={expected[i]}");

        await map.SetAsync(new Thing { Value = 2 }, 1);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(3));
        expected[2] = 2;
        for (var i = 0; i < tpcServerCount; i++)
            Assert.That(tpcServers[i].State.HandledMessages.Count, Is.EqualTo(expected[i]), $"Server {i} count={tpcServers[i].State.HandledMessages.Count} expected={expected[i]}");

        await map.SetAsync(new Thing { Value = 3 }, 1);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(3));
        expected[0] = 2;
        for (var i = 0; i < tpcServerCount; i++)
            Assert.That(tpcServers[i].State.HandledMessages.Count, Is.EqualTo(expected[i]), $"Server {i} count={tpcServers[i].State.HandledMessages.Count} expected={expected[i]}");

        await map.SetAsync(new Thing { Value = 4 }, 1);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(3));
        expected[1] = 3;
        for (var i = 0; i < tpcServerCount; i++)
            Assert.That(tpcServers[i].State.HandledMessages.Count, Is.EqualTo(expected[i]), $"Server {i} count={tpcServers[i].State.HandledMessages.Count} expected={expected[i]}");

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    [Test]
    [Timeout(30_000)]
    public async Task SendsRequestToClassicWhileWaitingForTpc()
    {
        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);

        classicServer.Handle(ClientCreateProxyCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            await request.RespondAsync(ClientCreateProxyServerCodec.EncodeResponse());
        });

        classicServer.Handle(MapSetCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            await request.RespondAsync(MapSetServerCodec.EncodeResponse());
        });

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        var tpcConnect = new ManualResetEventAsync(false);
        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens, tpcConnect);

        for (var i = 0; i < tpcServerCount; i++)
            tpcServers[i].Handle(MapSetCodec.RequestMessageType, async request =>
            {
                request.State.HandledMessages.Add(request.Message.MessageType);
                await request.RespondAsync(MapSetServerCodec.EncodeResponse());
            });

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        await using var client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));

        // but TPC connections are still pending
        for (var i = 0; i < tpcServerCount; i++) Assert.That(clientIds[i] == Guid.Empty);

        // client can function, will use classic connection
        var map = await client.GetMapAsync<int, int>("map");
        await map.SetAsync(1, 1);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(4));

        // unlock TPC connections: after a while, all TPC connections have been established
        tpcConnect.Set();
        await AssertEx.SucceedsEventually(() =>
        {
            for (var i = 0; i < tpcServerCount; i++)
            {
                Assert.That(clientIds[i], Is.EqualTo(client.Id));
                Assert.That(tokens[i], Is.EqualTo(tpcToken));
            }
        }, 10_000, 500);

        // and then ... use TPC
        await map.SetAsync(1, 1);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(4));
        for (var i = 0; i < tpcServerCount; i++) 
            Assert.That(tpcServers[i].State.HandledMessages.Count, Is.EqualTo(i == 0 ? 2 : 1));

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    [Test]
    [Timeout(30_000)]
    public async Task DoesHeartbeatTpc()
    {

        HConsole.Configure(x => x.ConfigureDefaults(this));

        const int tpcServerCount = 3;
        var classicPort = TestEndPointPort.GetNext();
        var tpcToken = new byte[64];
        new Random().NextBytes(tpcToken);
        var tpcPorts = new List<int>();
        for (var i = 0; i < tpcServerCount; i++) tpcPorts.Add(TestEndPointPort.GetNext());

        // create the classic server, handling the bare minimum so connection succeeds
        var classicServer = CreateClassicServer(classicPort, tpcPorts, tpcToken);
        classicServer.Handle(ClientPingCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            await request.RespondAsync(ClientPingServerCodec.EncodeResponse());
        });

        var clientIds = new Guid[tpcServerCount];
        var tokens = new byte[tpcServerCount][];

        // create the TPC servers, handling the bare minimum so connection succeeds
        var tpcServers = new Server<ServerState>[tpcServerCount];
        var tpcConnect = new ManualResetEventAsync(false);
        for (var i = 0; i < tpcServerCount; i++)
        {
            tpcServers[i] = CreateTpcServer(i, tpcPorts[i], clientIds, tokens, tpcConnect);
            tpcServers[i].Handle(ClientPingCodec.RequestMessageType, async request =>
            {
                request.State.HandledMessages.Add(request.Message.MessageType);
                await request.RespondAsync(ClientPingServerCodec.EncodeResponse());
            });
        }

        // start servers
        HConsole.WriteLine(this, "START CLASSIC");
        await classicServer.StartAsync();
        HConsole.WriteLine(this, "START TPC");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StartAsync();

        // configure and start client
        HConsole.WriteLine(this, "START CLIENT");
        var clientOptions = CreateHazelcastOptions(classicPort);
        clientOptions.Heartbeat.PeriodMilliseconds = 1000; // make sure to heartbeat
        await using var client = await HazelcastClientFactory.StartNewClientAsync(clientOptions);
        Assert.That(classicServer.State.HandledMessages.Count, Is.EqualTo(2));

        // after a while, all TPC connections have been established
        await AssertEx.SucceedsEventually(() =>
        {
            for (var i = 0; i < tpcServerCount; i++)
            {
                Assert.That(clientIds[i], Is.EqualTo(client.Id));
                Assert.That(tokens[i], Is.EqualTo(tpcToken));
            }
        }, 10_000, 500);

        // after a while, all servers have received a ping
        await AssertEx.SucceedsEventually(() =>
        {
            Assert.That(classicServer.State.HandledMessages.Contains(ClientPingCodec.RequestMessageType));
            for (var i = 0; i < tpcServerCount; i++)
                Assert.That(tpcServers[i].State.HandledMessages.Contains(ClientPingCodec.RequestMessageType));
        }, 10_000, 1_000);

        // stop everything
        HConsole.WriteLine(this, "STOP ALL");
        for (var i = 0; i < tpcServerCount; i++) await tpcServers[i].StopAsync();
        await classicServer.StopAsync();

        HConsole.WriteLine(this, "OVER AND OUT");
    }

    private class Thing : IPartitionAware
    {
        public int Value { get; init; }

        public object GetPartitionKey()
        {
            var bytes = new byte[16];
            bytes.WriteInt(0, Value, Endianness.BigEndian);
            var data = new HeapData(bytes);
            return data;
        }
    }

    private class ServerState
    {
        private readonly List<int> _handledMessages = new();

        public ICollection<int> HandledMessages => _handledMessages;
    }

    private static Server<ServerState> CreateClassicServer(int classicPort, IList<int> tpcPorts, byte[] tpcToken)
    {
        // create the classic server, handling the bare minimum so connection succeeds
        var server = new Server(NetworkAddress.Parse("127.0.0.1:" + classicPort), hcName: "CLASSIC")
            .WithState(new ServerState());

        server.Handle(TpcClientAuthenticationServerCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            _ = TpcClientAuthenticationServerCodec.DecodeRequest(request.Message);
            var responseMessage = TpcClientAuthenticationServerCodec.EncodeResponse(
                0,
                request.Server.Address, request.Server.MemberId,
                SerializationService.SerializerVersion,
                ClientVersion.Version,
                1,
                request.Server.ClusterId,
                false,
                tpcPorts,
                tpcToken
            );

            await request.RespondAsync(responseMessage);
        });

        server.Handle(ClientAddClusterViewListenerCodec.RequestMessageType, async request =>
        {
            request.State.HandledMessages.Add(request.Message.MessageType);
            _ = ClientAddClusterViewListenerServerCodec.DecodeRequest(request.Message);
            var responseMessage = ClientAddClusterViewListenerServerCodec.EncodeResponse();
            await request.RespondAsync(responseMessage);

            _ = Task.Run(async () =>
            {
                await Task.Delay(500).CfAwait();

                var memberVersion = new MemberVersion(4, 0, 0);
                var memberInfo = new MemberInfo(request.Server.MemberId, request.Server.Address, memberVersion, false, new Dictionary<string, string>());
                var eventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(1, new[] { memberInfo });
                await request.RaiseAsync(eventMessage);

                // pretend we have 3 partitions, all going to the same member = to each of the 3 TPC channels
                var partitions = new Dictionary<Guid, IList<int>>
                {
                    { request.Server.MemberId, new List<int> { 1, 2, 3 } }
                };
                eventMessage = ClientAddClusterViewListenerServerCodec.EncodePartitionsViewEvent(1, partitions);
                await request.RaiseAsync(eventMessage);
            });
        });

        return server;
    }

    private static Server<ServerState> CreateTpcServer(int tpcIndex, int tpcPort, Guid[] clientIds, byte[][] tokens, ManualResetEventAsync connect = null)
    {
        var server = new Server(NetworkAddress.Parse("127.0.0.1:" + tpcPort), hcName: "TPC" + tpcIndex)
            .WithState(new ServerState());

        server.Handle(TpcClientChannelAuthenticationServerCodec.RequestMessageType, async request =>
        {
            if (connect != null) await connect.WaitAsync(CancellationToken.None);
            request.State.HandledMessages.Add(request.Message.MessageType);
            var requestParameters = TpcClientChannelAuthenticationServerCodec.DecodeRequest(request.Message);
            clientIds[tpcIndex] = requestParameters.ClientId;
            tokens[tpcIndex] = requestParameters.Token;
            var responseMessage = TpcClientChannelAuthenticationServerCodec.EncodeResponse();
            await request.RespondAsync(responseMessage);
        });

        return server;
    }

    private static HazelcastOptions CreateHazelcastOptions(int classicPort)
    {
        return new HazelcastOptionsBuilder()
            .With(options =>
            {
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 1000;
                options.Networking.Addresses.Add("127.0.0.1:" + classicPort);
                options.Networking.Tpc.Enabled = true;
                options.Networking.ReconnectMode = ReconnectMode.DoNotReconnect;
                options.Heartbeat.PeriodMilliseconds = -1; // no heartbeat please
            })
            .WithHConsoleLogger()
            .Build();
    }
}
