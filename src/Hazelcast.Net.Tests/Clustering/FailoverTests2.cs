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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Hazelcast.Testing.TestServer;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering;

[TestFixture]
public class FailoverTests2 : HazelcastTestBase
{
    private readonly Dictionary<string, (string Key, string Value)> _kv = new();

    private class ServerState
    {
        private readonly Dictionary<string, Dictionary<IData, IData>> _maps = new();

        public int Id { get; init; }

        public bool SendMembershipEvents { get; set; } = true;

        public Dictionary<IData, IData> CreateMap(string name)
        {
            return _maps.TryGetValue(name, out var map)
                ? map
                : _maps[name] = new();
        }

        public bool TryGetMap(string name, out Dictionary<IData, IData> map)
            => _maps.TryGetValue(name, out map);
    }

    [Test]
    public async Task TestClientCanFailover()
    {
        // this is equivalent to FailoverTests.TestClientCanFailover but with our custom server

        var cluster0Id = Guid.NewGuid();
        var cluster1Id = Guid.NewGuid();

        _kv[cluster0Id.ToString()] = ("key0", "value0");
        _kv[cluster1Id.ToString()] = ("key1", "value1");

        var member0Address = NetworkAddress.Parse("127.0.0.1:5701");
        var state0 = new ServerState { Id = 0 };
        await using var member0 = new Server(member0Address)
            .WithClusterId(cluster0Id)
            .WithState(state0)
            .HandleFallback(ServerHandler);

        var member1Address = NetworkAddress.Parse("127.0.0.1:5711");
        var state1 = new ServerState { Id = 1 };
        await using var member1 = new Server(member1Address)
            .WithClusterId(cluster1Id)
            .WithState(state1)
            .HandleFallback(ServerHandler);

        HConsole.Configure(options => options
                .ConfigureDefaults(this)
                .Configure<Failover>().SetPrefix("FAILOVER").SetMaxLevel()
            );

        var states = new ConcurrentQueue<ClientState>();

        var failoverOptions = new HazelcastFailoverOptionsBuilder()
             .With(fo =>
             {
                 fo.TryCount = 2;

                 // first cluster is the primary, and able to configure everything
                 fo.Clients.Add(new HazelcastOptionsBuilder()
                     .With(o =>
                     {
                         // this applies to this cluster only
                         o.ClusterName = cluster0Id.ToString();
                         o.Networking.Addresses.Clear();
                         o.Networking.Addresses.Add(member0Address.ToString());
                         o.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
                         o.Networking.SmartRouting = true;

                         // each single socket connection attempt has a timeout
                         // connection to a cluster has a total timeout
                         o.Networking.ConnectionTimeoutMilliseconds = 4_000;
                         o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 8_000;

                         // this applies to all clusters
                         o.AddSubscriber(events =>
                             events.StateChanged((sender, arg) =>
                             {
                                 HConsole.WriteLine(this, $"State changed to: {arg.State}");
                                 states.Enqueue(arg.State);
                             }));
                     })
                     .Build());

                 // second cluster is alternate, only need to configure network
                 fo.Clients.Add(new HazelcastOptionsBuilder()
                     .With(o =>
                     {
                         o.ClusterName = cluster1Id.ToString();
                         o.Networking.Addresses.Clear();
                         o.Networking.Addresses.Add(member1Address.ToString());
                     })
                     .Build());
             })
             .WithHConsoleLogger()
             .Build();

        HConsole.WriteLine(this, "Start members of clusters 0 and 1");
        await member0.StartAsync();
        await member1.StartAsync();

        HConsole.WriteLine(this, "Start failover client");
        await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(failoverOptions);
        var mapName = CreateUniqueName();
        var map = await client.GetMapAsync<string, string>(mapName);
        Assert.IsNotNull(map);

        // first cluster should be 0
        FailoverTests.AssertState(states, ClientState.Starting);
        FailoverTests.AssertState(states, ClientState.Started);
        FailoverTests.AssertState(states, ClientState.Connected);
        await AssertCluster(client, cluster0Id.ToString(), map);

        // stop cluster 0 members
        HConsole.WriteLine(this, "Stop members of cluster 0");
        await member0.StopAsync();

        // we should disconnect
        await FailoverTests.AssertStateEventually(states, ClientState.Disconnected);

        // we should failover to cluster 1
        await FailoverTests.AssertStateEventually(states, ClientState.ClusterChanged);
        await FailoverTests.AssertStateEventually(states, ClientState.Connected);
        await AssertCluster(client, cluster1Id.ToString(), map);

        // start cluster 0 members again
        HConsole.WriteLine(this, "Start members of Cluster 0");
        await member0.StartAsync();

        // stop cluster 1 members
        HConsole.WriteLine(this, "Stop members of Cluster 1");
        await member1.StopAsync();

        // we should disconnect
        await FailoverTests.AssertStateEventually(states, ClientState.Disconnected);

        // we should failover to cluster 0
        await FailoverTests.AssertStateEventually(states, ClientState.ClusterChanged);
        await FailoverTests.AssertStateEventually(states, ClientState.Connected);
        await AssertCluster(client, cluster0Id.ToString(), map);
    }

    [Test]
    public async Task TestClientCanFailoverWhenNoInitialMembershipEvent()
    {
        // this is equivalent to FailoverTests.TestClientCanFailover but with our custom server
        // and, we tweak the server so it does *not* send initial membership event in time
        // see https://github.com/hazelcast/hazelcast/pull/26009

        var cluster0Id = Guid.NewGuid();
        var cluster1Id = Guid.NewGuid();
        var cluster2Id = Guid.NewGuid();

        _kv[cluster0Id.ToString()] = ("key0", "value0");
        _kv[cluster1Id.ToString()] = ("key1", "value1");
        _kv[cluster2Id.ToString()] = ("key2", "value2");

        var member0Address = NetworkAddress.Parse("127.0.0.1:5701");
        var state0 = new ServerState { Id = 0 };
        await using var member0 = new Server(member0Address)
            .WithClusterId(cluster0Id)
            .WithState(state0)
            .HandleFallback(ServerHandler);

        var member1Address = NetworkAddress.Parse("127.0.0.1:5711");
        var state1 = new ServerState { Id = 1 };
        await using var member1 = new Server(member1Address)
            .WithClusterId(cluster1Id)
            .WithState(state1)
            .HandleFallback(ServerHandler);

        var member2Address = NetworkAddress.Parse("127.0.0.1:5721");
        var state2 = new ServerState { Id = 2 };
        await using var member2 = new Server(member2Address)
            .WithClusterId(cluster2Id)
            .WithState(state2)
            .HandleFallback(ServerHandler);

        HConsole.Configure(options => options
                .ConfigureDefaults(this)
                .Configure<Failover>().SetPrefix("FAILOVER").SetMaxLevel()
            );

        var states = new ConcurrentQueue<ClientState>();

        var failoverOptions = new HazelcastFailoverOptionsBuilder()
             .With(fo =>
             {
                 fo.TryCount = 2;

                 // first cluster is the primary, and able to configure everything
                 fo.Clients.Add(new HazelcastOptionsBuilder()
                     .With(o =>
                     {
                         // this applies to this cluster only
                         o.ClusterName = cluster0Id.ToString();
                         o.Networking.Addresses.Clear();
                         o.Networking.Addresses.Add(member0Address.ToString());
                         o.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
                         o.Networking.SmartRouting = true;

                         // each single socket connection attempt has a timeout
                         // connection to a cluster has a total timeout
                         o.Networking.ConnectionTimeoutMilliseconds = 4_000;
                         o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 8_000;

                         // this applies to all clusters
                         o.AddSubscriber(events =>
                             events.StateChanged((sender, arg) =>
                             {
                                 HConsole.WriteLine(this, $"State changed to: {arg.State}");
                                 states.Enqueue(arg.State);
                             }));
                     })
                     .Build());

                 // second cluster is alternate, only need to configure network
                 fo.Clients.Add(new HazelcastOptionsBuilder()
                     .With(o =>
                     {
                         o.ClusterName = cluster1Id.ToString();
                         o.Networking.Addresses.Clear();
                         o.Networking.Addresses.Add(member1Address.ToString());
                     })
                     .Build());
                 fo.Clients.Add(new HazelcastOptionsBuilder()
                     .With(o =>
                     {
                         o.ClusterName = cluster2Id.ToString();
                         o.Networking.Addresses.Clear();
                         o.Networking.Addresses.Add(member2Address.ToString());
                     })
                     .Build());
             })
             .WithHConsoleLogger()
             .Build();

        HConsole.WriteLine(this, "Start members of clusters 0, 1 and 2");
        await member0.StartAsync();
        await member1.StartAsync();
        await member2.StartAsync();

        HConsole.WriteLine(this, "Start failover client");
        await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(failoverOptions);
        var mapName = CreateUniqueName();
        var map = await client.GetMapAsync<string, string>(mapName);
        Assert.IsNotNull(map);

        // first cluster should be 0
        FailoverTests.AssertState(states, ClientState.Starting);
        FailoverTests.AssertState(states, ClientState.Started);
        FailoverTests.AssertState(states, ClientState.Connected);
        await AssertCluster(client, cluster0Id.ToString(), map);

        // test: we provoke a failover to cluster 1, but that cluster is not
        // going to send the membership events, and then we're going to stop
        // it - we should failover back to cluster 2.

        state1.SendMembershipEvents = false;

        // stop cluster 0 members
        HConsole.WriteLine(this, "Stop members of cluster 0");
        await member0.StopAsync();

        // we should disconnect
        await FailoverTests.AssertStateEventually(states, ClientState.Disconnected);

        // we should failover to cluster 2, after failing to failover to cluster 1
        await FailoverTests.AssertStateEventually(states, ClientState.ClusterChanged, 20_000);
        await FailoverTests.AssertStateEventually(states, ClientState.Connected, 20_000);
        await AssertCluster(client, cluster2Id.ToString(), map);
    }

    private IData NullData { get; } = new HeapData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }); // NULL type id is zero

    private async ValueTask ServerHandler(ClientRequest<ServerState> request)
    {
        const int partitionsCount = 2;
        var address = request.Server.Address;
        var memberId = request.Server.MemberId;

        switch (request.Message.MessageType)
        {
            // must handle auth
            case ClientAuthenticationServerCodec.RequestMessageType:
                {
                    HConsole.WriteLine(this, $"(server{request.State.Id}) Authentication");
                    var authRequest = ClientAuthenticationServerCodec.DecodeRequest(request.Message);
                    var authResponse = ClientAuthenticationServerCodec.EncodeResponse(
                        0, address, memberId, SerializationService.SerializerVersion,
                        "4.0", partitionsCount, request.Server.ClusterId, true,
                        Array.Empty<int>(), Array.Empty<byte>(),
                        0, Array.Empty<MemberInfo>(),
                        0, Array.Empty<KeyValuePair<Guid, IList<int>>>());
                    await request.RespondAsync(authResponse).CfAwait();
                    break;
                }

            // must handle events
            case ClientAddClusterViewListenerServerCodec.RequestMessageType:
                {
                    HConsole.WriteLine(this, $"(server{request.State.Id}) AddClusterViewListener");
                    var addRequest = ClientAddClusterViewListenerServerCodec.DecodeRequest(request.Message);
                    var addResponse = ClientAddClusterViewListenerServerCodec.EncodeResponse();
                    await request.RespondAsync(addResponse).CfAwait();

                    if (request.State.SendMembershipEvents)
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(500).CfAwait();

                            const int membersVersion = 1;
                            var memberVersion = new MemberVersion(4, 0, 0);
                            var memberAttributes = new Dictionary<string, string>();
                            var membersEventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(membersVersion, new[]
                            {
                                    new MemberInfo(memberId, address, memberVersion, false, memberAttributes)
                                });
                            await request.RaiseAsync(membersEventMessage).CfAwait();

                            await Task.Delay(500).CfAwait();

                            const int partitionsVersion = 1;
                            var partitionsEventMessage = ClientAddClusterViewListenerServerCodec.EncodePartitionsViewEvent(partitionsVersion, new[]
                            {
                                    new KeyValuePair<Guid, IList<int>>(memberId, new List<int> { 0 }),
                                    new KeyValuePair<Guid, IList<int>>(memberId, new List<int> { 1 }),
                                });
                            await request.RaiseAsync(partitionsEventMessage).CfAwait();
                        });

                    break;
                }

            // ping
            case ClientPingServerCodec.RequestMessageType:
                {
                    HConsole.WriteLine(this, $"(server{request.State.Id}) Ping");
                    var pingRequest = ClientPingServerCodec.DecodeRequest(request.Message);

                    // no response, will timeout

                    break;
                }

            // create object
            case ClientCreateProxyCodec.RequestMessageType:
            {
                HConsole.WriteLine(this, $"(server{request.State.Id}) CreateProxy");
                var createProxyRequest = ClientCreateProxyServerCodec.DecodeRequest(request.Message);
                var service = createProxyRequest.ServiceName;
                if (service != ServiceNames.Map)
                {
                    await request.ErrorAsync(RemoteError.ServiceNotFound);
                }
                else
                {
                    var name = createProxyRequest.Name;
                    request.State.CreateMap(name);
                    await request.RespondAsync(ClientCreateProxyServerCodec.EncodeResponse());
                }
                break;
            }

            // create objects (when re-connecting)
            case ClientCreateProxiesCodec.RequestMessageType:
            {
                HConsole.WriteLine(this, $"(server{request.State.Id}) CreateProxies");
                var createProxiesRequest = ClientCreateProxiesServerCodec.DecodeRequest(request.Message);
                // do nothing - will work anyways
                await request.RespondAsync(ClientCreateProxiesServerCodec.EncodeResponse());
                break;
            }

            // map put
            case MapPutCodec.RequestMessageType:
            {
                HConsole.WriteLine(this, $"(server{request.State.Id}) MapPut");
                var mapPutRequest = MapPutServerCodec.DecodeRequest(request.Message);
                if (!request.State.TryGetMap(mapPutRequest.Name, out var map))
                {
                    map = request.State.CreateMap(mapPutRequest.Name);
                }

                var keyData = mapPutRequest.Key;
                if (!map.TryGetValue(keyData, out var responseData))
                    responseData = NullData;

                var valueData = mapPutRequest.Value;
                map[keyData] = valueData;

                await request.RespondAsync(MapPutServerCodec.EncodeResponse(responseData));
                break;
            }

            // map get
            case MapGetCodec.RequestMessageType:
            {
                HConsole.WriteLine(this, $"(server{request.State.Id}) MapGet");
                var mapGetRequest = MapGetServerCodec.DecodeRequest(request.Message);
                if (!request.State.TryGetMap(mapGetRequest.Name, out var map))
                {
                    map = request.State.CreateMap(mapGetRequest.Name);
                }

                var keyData = mapGetRequest.Key;
                if (!map.TryGetValue(keyData, out var responseData))
                    responseData = NullData;

                await request.RespondAsync(MapGetServerCodec.EncodeResponse(responseData));
                break;
            }

            // unexpected message = error
            default:
                {
                    // RemoteError.Hazelcast or RemoteError.RetryableHazelcast
                    var messageName = MessageTypeConstants.GetMessageTypeName(request.Message.MessageType);
                    await request.ErrorAsync(RemoteError.Hazelcast, $"MessageType {messageName} (0x{request.Message.MessageType:X}) not implemented.").CfAwait();
                    break;
                }
        }
    }

    /// <summary>
    /// Asserts that the client is connected to the expected cluster & can access the specified map.
    /// </summary>
    public async Task AssertCluster(IHazelcastClient client, string expectedClusterName, IHMap<string, string> map)
    {
        Assert.AreEqual(expectedClusterName, client.ClusterName);

        var key = _kv[expectedClusterName].Key;
        var value = _kv[expectedClusterName].Value;
        var otherKey = "";
        //var otherValue = "";
        foreach (var kv in _kv) 
        {
            if (kv.Key != expectedClusterName)
            {
                otherKey = kv.Value.Key;
                //otherValue = kv.Value.Value;
            }
        }

        // can use the map
        await map.PutAsync(key, value);
        Assert.AreEqual(value, await map.GetAsync(key));

        // cannot access other value
        Assert.IsNull(await map.GetAsync(otherKey));
    }

}