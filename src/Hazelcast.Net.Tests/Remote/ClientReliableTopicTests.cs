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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Remote;
using Hazelcast.Tests;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote;

public class ClientReliableTopicTests : SingleClusterNameKeptRemoteBase
{
    protected override string RcClusterConfiguration => Resources.Cluster_ReliableTopic;

    [Test]
    [Timeout(20_000)]
    public async Task TestReliableTopic([Values] TopicOverloadPolicy policy)
    {
        var topicName = "rtTestTopic";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(policy, 3);
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();


        var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);

        var msgCount = 3;
        var receivedCount = 0;
        var mneDisposed = new ManualResetEvent(false);
        var mne = new ManualResetEvent(false);
        var list = new int[msgCount + 1];

        await rt.SubscribeAsync(events =>
                events.Message((sender, args) =>
                {
                    list[args.Payload] = args.Payload;
                    HConsole.WriteLine(this, "SEQ:" + args.Sequence);
                    HConsole.WriteLine(this, "Data:" + args.Payload);
                    receivedCount++;
                    if (args.Payload == msgCount)
                        mne.Set();
                }).Disposed((sender, args) =>
                    {
                        HConsole.WriteLine(this, "Reliable topic subscription is disposed.");
                        mneDisposed.Set();
                    }
                ),
            new ReliableTopicEventHandlerOptions() {IsLossTolerant = false},
            ex =>
            {
                HConsole.WriteLine(this, ex);
                return true;
            });

        // 0 is not a message.
        for (var i = 1; i <= msgCount; i++)
            await rt.PublishAsync(i);

        var result = await mne.WaitOneAsync();
        Assert.True(result);

        var assertedCount = 0;
        for (int i = 1; i <= msgCount; i++)
        {
            if (list[i] == i) assertedCount++;
        }

        Assert.AreEqual(receivedCount, assertedCount);
        await rt.DestroyAsync();
        Assert.True(await mneDisposed.WaitOneAsync());
    }

    [Test]
    [Timeout(60_000)]
    public async Task TestReliableTopicBlocking()
    {
        //HConsoleForTest();
        var topicName = "rtTestTopicBlocking";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.Block, 3);
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();


        var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);

        var msgCount = 7;
        var receivedCount = 0;
        var mneDisposed = new ManualResetEvent(false);

        await rt.SubscribeAsync(events =>
                events.Message((sender, args) => { Interlocked.Increment(ref receivedCount); })
                    .Disposed((sender, args) => mneDisposed.Set()),
            new ReliableTopicEventHandlerOptions() {IsLossTolerant = false},
            ex =>
            {
                HConsole.WriteLine(this, ex);
                return true;
            });

        // Ring buffer has 5 item capacity but eventually 7 item will be published since older ones have TTL.
        for (var i = 0; i < msgCount; i++)
            await rt.PublishAsync(i);

        await AssertEx.SucceedsEventually(() => Assert.AreEqual(msgCount, receivedCount), 30_000, 200);

        await rt.DestroyAsync();
        await AssertEx.SucceedsEventually(async () => Assert.True(await mneDisposed.WaitOneAsync()), 20_000, 200);
    }

    [Test]
    [Timeout(20_000)]
    public async Task TestReliableTopicWhenNoSpaceOnError()
    {
        var topicName = "rtNoSpaceError";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.Error, 1);
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();

        var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);

        var rb = await client.GetRingBufferAsync<ReliableTopicMessage>("_hz_rb_" + topicName);


        // Ring buffer has 10 item capacity.
        for (var i = 0; i < 10; i++)
            await rt.PublishAsync(i);

        var tail = await rb.GetTailSequenceAsync();
        var head = await rb.GetHeadSequenceAsync();

        Assert.ThrowsAsync<TopicOverloadException>(async () => await rt.PublishAsync(11));

        Assert.AreEqual(tail, await rb.GetTailSequenceAsync());
        Assert.AreEqual(head, await rb.GetHeadSequenceAsync());
    }

    [Test]
    [Timeout(20_000)]
    public async Task TestReliableTopicWhenNoSpaceOnDiscardOldest()
    {
        var topicName = "rtNoSpaceDiscardOldest";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.DiscardOldest, 1);
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();

        var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);

        var rb = await client.GetRingBufferAsync<ReliableTopicMessage>("_hz_rb_" + topicName);

        // Ring buffer has 10 item capacity.
        for (var i = 0; i < 10; i++)
            await rt.PublishAsync(i);

        var tail = await rb.GetTailSequenceAsync();
        var head = await rb.GetHeadSequenceAsync();

        await rt.PublishAsync(10);

        Assert.AreEqual(tail + 1, await rb.GetTailSequenceAsync());
        Assert.AreEqual(head + 1, await rb.GetHeadSequenceAsync());
    }

    [Test]
    [Timeout(20_000)]
    public async Task TestReliableTopicWhenNoSpaceOnDiscardNewest()
    {
        var topicName = "rtNoSpaceDiscardNewest";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.DiscardNewest, 1);
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();

        var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);

        var rb = await client.GetRingBufferAsync<ReliableTopicMessage>("_hz_rb_" + topicName);


        // Ring buffer has 10 item capacity.
        for (var i = 0; i < 10; i++)
            await rt.PublishAsync(i);

        var tail = await rb.GetTailSequenceAsync();
        var head = await rb.GetHeadSequenceAsync();

        await rt.PublishAsync(10);

        Assert.AreEqual(tail, await rb.GetTailSequenceAsync());
        Assert.AreEqual(head, await rb.GetHeadSequenceAsync());
    }

    [Test]
    [Timeout(20_000)]
    public async Task TestReliableTopicWhenNoSpaceOnBlock()
    {
        var topicName = "rtNoSpaceBlock";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.Block, 1);
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();

        var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);

        var rb = await client.GetRingBufferAsync<ReliableTopicMessage>("_hz_rb_" + topicName);

        // Ring buffer has 10 item capacity.
        for (var i = 0; i < 10; i++)
            await rt.PublishAsync(i);

        var tail = await rb.GetTailSequenceAsync();
        var head = await rb.GetHeadSequenceAsync();

        var tAdd = rt.PublishAsync(10);

        var t = await Task.WhenAny(tAdd, Task.Delay(3_000));

        Assert.AreNotEqual(tAdd, t);

        Assert.AreEqual(tail, await rb.GetTailSequenceAsync());
        Assert.AreEqual(head, await rb.GetHeadSequenceAsync());
    }

    [Test]
    [Timeout(20_000)]
    public async Task TestReliableTopicWithNullParams()
    {
        var topicName = "rtTestTopicNull";


        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.Block, 3);
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();

        var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<string>(topicName);

        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await rt.SubscribeAsync(null));
        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await rt.SubscribeAsync(events => events.Message(null)));
        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await rt
            .SubscribeAsync(events => events
                .Message((sender, args) => { })
                .Disposed(null)
            ));

        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await rt.PublishAsync(null));
    }

    [Test]
    [Timeout(60_000)]
    public async Task TestClusterRestartWhenSubscribed()
    {
        var topicName = "rtTestTopic2";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.Block, 1);

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();

        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        await using var client2 = await HazelcastClientFactory.StartNewClientAsync(options);
        await using var rt = await client.GetReliableTopicAsync<int>(topicName);

        var rt2 = await client2.GetReliableTopicAsync<int>(topicName);
        var mne = new ManualResetEvent(false);

        await rt.SubscribeAsync(events =>
                events.Message((sender, args) =>
                {
                    Console.WriteLine("Message:" + args.Payload);
                    mne.Set();
                }),
            new ReliableTopicEventHandlerOptions() {IsLossTolerant = true},
            ex =>
            {
                HConsole.WriteLine(this, ex);
                return false;
            });

        await RestartCluster(async () =>
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 10_000, 100));

        await rt2.PublishAsync(1);

        Assert.True(await mne.WaitOneAsync());
    }

    [Test]
    [Timeout(60_000)]
    public async Task TestClusterRestartAfterInvocationTimeOut()
    {
        var topicName = "rtTestTopic3";
        var timeOut = 2_000;

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");
                opt.Messaging.RetryTimeoutSeconds = timeOut;
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.Block, 1);

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();

        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        await using var rt = await client.GetReliableTopicAsync<int>(topicName);

        var mne = new ManualResetEvent(false);

        var sId = await rt.SubscribeAsync(events =>
                events.Message((sender, args) =>
                {
                    Console.WriteLine("Message:" + args.Payload);
                    mne.Set();
                }),
            new ReliableTopicEventHandlerOptions() {IsLossTolerant = true},
            ex =>
            {
                HConsole.WriteLine(this, ex);
                return false;
            });

        await RcClient.StopMemberWaitClosedAsync(client, RcCluster, RcMember);

        await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 5_000, 100);

        await Task.Delay(timeOut);

        await MemberSetUp();

        await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(99)", Lang.JAVASCRIPT);

        Assert.True(await mne.WaitOneAsync());
        var rtImpl = (HReliableTopic<int>) rt;
        Assert.True(rtImpl.TryGetExecutor(sId, out var exc));
        Assert.False(exc.IsDisposed);
    }

    [Test]
    [Timeout(60_000)]
    public async Task TestClusterRestartWhenNotLossTolerant()
    {
        //HConsoleForTest();
        var topicName = "rtNotTolerant";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ClusterName = RcCluster.Id;
                opt.Networking.Reconnect = true;
                opt.Networking.Addresses.Add("127.0.0.1:5704");
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.DiscardOldest, 1);

                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConfiguration(conf.GetSection("logging"));
                    builder.AddConsole();
                });
            })
            .Build();

        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        await using var rt = await client.GetReliableTopicAsync<int>(topicName);

        for (int i = 0; i < 99; i++)
            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(" + i + ")", Lang.JAVASCRIPT);

        var msgReceivedCount = 0;

        var rtEventOpt = new ReliableTopicEventHandlerOptions() {IsLossTolerant = false, InitialSequence = -1, StoreSequence = true};

        var sId = await rt.SubscribeAsync(events =>
            events.Message((sender, args) =>
            {
                rtEventOpt.InitialSequence = args.Sequence;
                Console.WriteLine("SEQ:" + args.Sequence);
                Console.WriteLine("Message:" + args.Payload);
                Interlocked.Increment(ref msgReceivedCount);
            }), rtEventOpt);

        // Let subscriber receives the sequence before restart.
        await Task.Delay(500);

        await RestartCluster(async () =>
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 10_000, 100));

        // Cluster restarted, sequence and data should be lost. The subscriber cannot continue since it is not loss tolerant.
        for (int i = 0; i < 100; i++)
        {
            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(" + (i + 100) + ")", Lang.JAVASCRIPT);
        }

        await AssertEx.SucceedsEventually(async () =>
        {
            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(101)", Lang.JAVASCRIPT);
            Assert.False(rt.IsSubscriptionExist(sId));
        }, 10_000, 100);

        Assert.AreEqual(0, msgReceivedCount);

        var rtImpl = (HReliableTopic<int>) rt;
        Assert.AreEqual(0, msgReceivedCount);
    }
}
