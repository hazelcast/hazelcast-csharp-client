﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Protocol;
using Hazelcast.Testing.TestServer;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering
{
    [TestFixture]
    [Timeout(30_000)]
    public class SubscriptionCollectTests
    {
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                .Set(x => x.Verbose())
                .Set(this, x => x.SetPrefix("TEST"))
                .Set<AsyncContext>(x => x.Quiet())
                .Set<SocketConnectionBase>(x => x.SetIndent(1).SetLevel(0).SetPrefix("SOCKET")));

        [Test]
        public async Task SubscriptionIsCollected()
        {
            var address0 = NetworkAddress.Parse("127.0.0.1:11001");
            var address1 = NetworkAddress.Parse("127.0.0.1:11002");

            var memberId0 = Guid.NewGuid();
            var memberId1 = Guid.NewGuid();

            using var __ = HConsoleForTest();

            HConsole.WriteLine(this, "Begin");
            HConsole.WriteLine(this, "Start servers");

            var loggerFactory = new NullLoggerFactory();

            var state0 = new ServerState
            {
                Id = 0,
                MemberIds = new[] { memberId0, memberId1 },
                Addresses = new[] { address0, address1 },
                MemberId = memberId0,
                Address = address0
            };

            await using var server0 = new Server(address0, ServerHandler, loggerFactory, state0, "0")
            {
                MemberId = state0.MemberId,
            };
            await server0.StartAsync();

            var state1 = new ServerState
            {
                Id = 1,
                MemberIds = new[] { memberId0, memberId1 },
                Addresses = new[] { address0, address1 },
                MemberId = memberId1,
                Address = address1
            };

            await using var server1 = new Server(address1, ServerHandler, loggerFactory, state1, "1")
            {
                MemberId = state1.MemberId,
                ClusterId = server0.ClusterId
            };
            await server1.StartAsync();

            HConsole.WriteLine(this, "Start client");

            var options = HazelcastOptions.Build(configure: (configuration, options) =>
            {
                options.Networking.Addresses.Add("127.0.0.1:11001");
                options.Networking.Addresses.Add("127.0.0.1:11002");
                options.Events.SubscriptionCollectDelay = TimeSpan.FromSeconds(4); // don't go too fast
            });
            await using var client = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);

            HConsole.WriteLine(this, "Get map");

            var map = await client.GetMapAsync<string, string>("name");
            var count = 0;

            var clusterEvents = client.Cluster.Events;
            Assert.That(clusterEvents.Subscriptions.Count, Is.EqualTo(0)); // no client subscription yet
            Assert.That(clusterEvents.CorrelatedSubscriptions.Count, Is.EqualTo(1)); // but the cluster views subscription

            HConsole.WriteLine(this, "Subscribe");

            var sid = await map.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref count))
            );

            Assert.That(clusterEvents.Subscriptions.Count, Is.EqualTo(1)); // 1 (our) client subscription
            Assert.That(clusterEvents.Subscriptions.TryGetValue(sid, out var subscription)); // can get our subscription

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(clusterEvents.CorrelatedSubscriptions.Count, Is.EqualTo(3)); // 2 more correlated
                Assert.That(subscription.Count, Is.EqualTo(2)); // 2 members
                Assert.That(subscription.Active);
            }, 4000, 200);

            HConsole.WriteLine(this, "Set");

            await map.SetAsync("key", "value");
            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(count, Is.EqualTo(1)); // event triggered
            }, 2000, 100);

            HConsole.WriteLine(this, "Unsubscribe");

            var unsubscribed = await map.UnsubscribeAsync(sid);
            Assert.That(unsubscribed);

            // we have a 4 sec delay before the collect task actually collects

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(subscription.Active, Is.False, "active");
                Assert.That(clusterEvents.Subscriptions.Count, Is.EqualTo(0), "count.1"); // is gone
                Assert.That(clusterEvents.CorrelatedSubscriptions.Count, Is.EqualTo(1), "count.2"); // are gone
                Assert.That(subscription.Count, Is.EqualTo(1), "count.3"); // 1 remains
                Assert.That(clusterEvents.CollectSubscriptions.Count, Is.EqualTo(1), "count.4"); // is ghost
            }, 4000, 200);

            // get a key that targets server 1 - the one that's going to send the event
            var key = GetKey(1, 2, client.SerializationService);

            HConsole.WriteLine(this, "Set key=" + key);

            await map.SetAsync(key, "value");
            await Task.Delay(100);
            Assert.That(count, Is.EqualTo(1)); // no event

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(subscription.Count, Is.EqualTo(0)); // 0 remains
                Assert.That(clusterEvents.CollectSubscriptions.Count, Is.EqualTo(0)); // is gone
            }, 8000, 200);
        }

        private static string GetKey(int partitionId, int partitionCount, SerializationService serializationService)
        {
            int GetHash(string value) => serializationService.ToData(value).PartitionHash;

            var key = "key0";
            for (var i = 1; i < 100 && GetHash(key) % partitionCount != partitionId; i++) key = "key" + i;
            return key;
        }

        private class ServerState
        {
            public Guid[] MemberIds { get; set; }
            public NetworkAddress[] Addresses { get; set; }
            public int Id { get; set; }
            public Guid MemberId { get; set; }
            public NetworkAddress Address { get; set; }
            public bool Subscribed { get; set; }
            public Guid SubscriptionId { get; set; }
            public int UnsubscribeCount { get; set; }
            public long SubscriptionCorrelationId { get; set; }
        }

        private async ValueTask ServerHandler(Server s, ClientMessageConnection conn, ClientMessage msg)
        {
            async Task SendResponseAsync(ClientMessage response)
            {
                response.CorrelationId = msg.CorrelationId;
                response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                await conn.SendAsync(response).CfAwait();
            }

            async Task SendEventAsync(ClientMessage eventMessage, long correlationId)
            {
                eventMessage.CorrelationId = correlationId;
                eventMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                await conn.SendAsync(eventMessage).CfAwait();
            }

            async Task SendErrorAsync(RemoteError error, string message)
            {
                var errorHolders = new List<ErrorHolder>
                    {
                        new ErrorHolder(error, "?", message, Enumerable.Empty<StackTraceElement>())
                    };
                var response = ErrorsServerCodec.EncodeResponse(errorHolders);
                await SendResponseAsync(response).CfAwait();
            }

            var state = (ServerState) s.State;
            var address = s.Address;

            const int partitionsCount = 2;

            switch (msg.MessageType)
            {
                // must handle auth
                case ClientAuthenticationServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) Authentication");
                        var authRequest = ClientAuthenticationServerCodec.DecodeRequest(msg);
                        var authResponse = ClientAuthenticationServerCodec.EncodeResponse(
                            0, address, s.MemberId, SerializationService.SerializerVersion,
                            "4.0", partitionsCount, s.ClusterId, false);
                        await SendResponseAsync(authResponse).CfAwait();
                        break;
                    }

                // must handle events
                case ClientAddClusterViewListenerServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) AddClusterViewListener");
                        var addRequest = ClientAddClusterViewListenerServerCodec.DecodeRequest(msg);
                        var addResponse = ClientAddClusterViewListenerServerCodec.EncodeResponse();
                        await SendResponseAsync(addResponse).CfAwait();

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(500).CfAwait();

                            const int membersVersion = 1;
                            var memberVersion = new MemberVersion(4, 0, 0);
                            var memberAttributes = new Dictionary<string, string>();
                            var membersEventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(membersVersion, new[]
                            {
                                new MemberInfo(state.MemberIds[0], state.Addresses[0], memberVersion, false, memberAttributes),
                                new MemberInfo(state.MemberIds[1], state.Addresses[1], memberVersion, false, memberAttributes),
                            });
                            await SendEventAsync(membersEventMessage, msg.CorrelationId).CfAwait();

                            await Task.Delay(500).CfAwait();

                            const int partitionsVersion = 1;
                            var partitionsEventMessage = ClientAddClusterViewListenerServerCodec.EncodePartitionsViewEvent(partitionsVersion, new[]
                            {
                                new KeyValuePair<Guid, IList<int>>(state.MemberIds[0], new List<int> { 0 }),
                                new KeyValuePair<Guid, IList<int>>(state.MemberIds[1], new List<int> { 1 }),
                            });
                            await SendEventAsync(partitionsEventMessage, msg.CorrelationId).CfAwait();
                        });

                        break;
                    }

                // create object
                case ClientCreateProxyServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) CreateProxy");
                        var createRequest = ClientCreateProxyServerCodec.DecodeRequest(msg);
                        var createResponse = ClientCreateProxiesServerCodec.EncodeResponse();
                        await SendResponseAsync(createResponse).CfAwait();
                        break;
                    }

                // subscribe
                case MapAddEntryListenerServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) AddEntryListener");
                        var addRequest = MapAddEntryListenerServerCodec.DecodeRequest(msg);
                        state.Subscribed = true;
                        state.SubscriptionCorrelationId = msg.CorrelationId;
                        var addResponse = MapAddEntryListenerServerCodec.EncodeResponse(state.SubscriptionId);
                        await SendResponseAsync(addResponse).CfAwait();
                        break;
                    }

                // unsubscribe
                // server 1 removes on first try, server 2 removes on later tries
                case MapRemoveEntryListenerServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) RemoveEntryListener");
                        var removeRequest = MapRemoveEntryListenerServerCodec.DecodeRequest(msg);
                        var removed = state.Subscribed && removeRequest.RegistrationId == state.SubscriptionId;
                        removed &= state.Id == 0 || state.UnsubscribeCount++ > 0;
                        if (removed) state.Subscribed = false;
                        HConsole.WriteLine(this, $"(server{state.Id}) Subscribed={state.Subscribed}");
                        var removeResponse = MapRemoveEntryListenerServerCodec.EncodeResponse(removed);
                        await SendResponseAsync(removeResponse).CfAwait();
                        break;
                    }

                // add to map & trigger event
                case MapSetServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) Set");
                        var setRequest = MapSetServerCodec.DecodeRequest(msg);
                        var setResponse = MapSetServerCodec.EncodeResponse();
                        await SendResponseAsync(setResponse).CfAwait();

                        HConsole.WriteLine(this, $"(server{state.Id}) Subscribed={state.Subscribed}");

                        if (state.Subscribed)
                        {
                            HConsole.WriteLine(this, $"(server{state.Id}) Trigger event");
                            var key = setRequest.Key;
                            var value = setRequest.Value;
                            var addedEvent = MapAddEntryListenerServerCodec.EncodeEntryEvent(key, value, value, value, (int)MapEventTypes.Added, state.SubscriptionId, 1);
                            await SendEventAsync(addedEvent, state.SubscriptionCorrelationId).CfAwait();
                        }
                        break;
                    }

                // unexpected message = error
                default:
                    {
                        // RemoteError.Hazelcast or RemoteError.RetryableHazelcast
                        var messageName = MessageTypeConstants.GetMessageTypeName(msg.MessageType);
                        await SendErrorAsync(RemoteError.Hazelcast, $"MessageType {messageName} (0x{msg.MessageType:X}) not implemented.").CfAwait();
                        break;
                    }
            }
        }
    }
}
