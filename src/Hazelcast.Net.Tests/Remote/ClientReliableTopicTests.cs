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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote;

public class ClientReliableTopicTests : SingleMemberRemoteTestBase
{
    // Remarks on #7317 (https://github.com/hazelcast/hazelcast/issues/7317)
    // The issue is fixed by reading the head sequence from the ring buffer which is the current implementation.

    protected override string RcClusterConfiguration => Resources.Cluster_ReliableTopic;
    protected override bool KeepClusterName => true;

    private async Task RestartCluster(Func<ValueTask> runAfterShutdown = default, Cluster cluster = default)
    {
        cluster ??= RcCluster;

        await MemberOneTimeTearDown();
        await RcClient.ShutdownClusterAsync(cluster.Id);
        if (runAfterShutdown != default) await runAfterShutdown();
        RcCluster = await RcClient.CreateClusterKeepClusterNameAsync(ServerVersion.DefaultVersion.Version.ToString(), RcClusterConfiguration).CfAwait();
        await MemberOneTimeSetUp();
    }

    [Test]
    [Timeout(80_000)]
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
                }).Terminated((sender, args) =>
                    {
                        HConsole.WriteLine(this, "Reliable topic subscription is disposed.");
                        mneDisposed.Set();
                    }
                ).Exception((sender, args) => { args.Cancel = true; }),
            new ReliableTopicEventHandlerOptions() {IsLossTolerant = false});

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
    [Timeout(80_000)]
    public async Task TestReliableTopicBlocking()
    {
        var topicName = "rtTestTopicBlocking";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(TopicOverloadPolicy.Block, 5);
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
                events.Message((sender, args) =>
                    {
                        Interlocked.Increment(ref receivedCount);
                        Console.WriteLine($"SEQ: {args.Sequence} Received:{args.Payload}");
                    })
                    .Terminated((sender, args) => mneDisposed.Set()),
            new ReliableTopicEventHandlerOptions() {IsLossTolerant = false, InitialSequence = -1, StoreSequence = false});

        // Let listener be ready.
        await Task.Delay(1_000);

        // Ring buffer has 5 item capacity but eventually 7 item will be published since older ones have TTL.
        for (var i = 0; i < msgCount; i++)
        {
            await rt.PublishAsync(i);
            Console.WriteLine("Published:" + i);
        }

        await AssertEx.SucceedsEventually(() => Assert.AreEqual(msgCount, receivedCount), 30_000, 200);

        await rt.DestroyAsync();
        await AssertEx.SucceedsEventually(async () => Assert.True(await mneDisposed.WaitOneAsync()), 30_000, 200);
    }

    [Test]
    [Timeout(80_000)]
    public async Task TestReliableTopicEventArguments()
    {
        var topicName = "rtTestTopicArgs";

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

        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);
  
        var mne = new ManualResetEvent(false);

        long publishTime = long.MaxValue;
        await rt.SubscribeAsync(events =>
            events.Message((sender, args) =>
            {
                publishTime = args.PublishTime;
                Assert.AreEqual(1, args.Payload);
                Assert.AreEqual(0, args.Sequence);
                mne.Set();
            }));

        // Be sure subscription is started.
        await Task.Delay(1_000);

        var beforePublish = Clock.Milliseconds;
        await rt.PublishAsync(1);
        var afterPublish = Clock.Milliseconds;
        Assert.True(await mne.WaitOneAsync());
        Assert.GreaterOrEqual(publishTime, beforePublish);
        Assert.LessOrEqual(publishTime, afterPublish);

        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(80_000)]
    public async Task TestReliableTopicExceptionEvent()
    {
        var topicName = "rtTestTopicArgs";

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


        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);
        var mneException = new ManualResetEvent(false);

        var mneContinue = new ManualResetEvent(false);

        await rt.SubscribeAsync(events =>
            events.Message((sender, args) =>
            {
                if (args.Payload == 1)
                    throw new ArgumentOutOfRangeException();
                else
                    mneContinue.Set();

            }).Exception((sender, args) =>
            {
                Assert.IsInstanceOf<ArgumentOutOfRangeException>(args.Exception);
                args.Cancel = false;
                mneException.Set();
            }));

        await rt.PublishAsync(1);
        Assert.True(await mneException.WaitOneAsync());
        for (int i = 2; i < 20; i++)
            await rt.PublishAsync(i);

        Assert.True(await mneContinue.WaitOneAsync());
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(80_000)]
    public async Task TestReliableTopicSubscribeReceiveEventsOnlyAfter()
    {
        var topicName = "rtTestTopicReceiveAfter";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
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

        // these shouldn't be consumed by listener.
        await rt.PublishAsync(1);
        await rt.PublishAsync(2);
        await rt.PublishAsync(3);

        var expected = new int[] {4, 5, 6};
        var received = new List<int>();
        var mneDone = new ManualResetEvent(false);
        
        await rt.SubscribeAsync(events =>
            events.Message((sender, args) =>
            {
                received.Add(args.Payload);
                if (received.Count == 3)
                    mneDone.Set();

            }));

        // Let subscriber get the sequence.
        await Task.Delay(2_000);

        await rt.PublishAsync(4);
        await rt.PublishAsync(5);
        await rt.PublishAsync(6);

        await mneDone.WaitOneAsync();
        Assert.AreEqual(expected, received.ToArray());
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(30_000)]
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
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(30_000)]
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
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(30_000)]
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
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(30_000)]
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
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(30_000)]
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
                .Terminated(null)
            ));

        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await rt.PublishAsync(null));
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(80_000)]
    public async Task TestSubscriberLivesWhenClientOffline()
    {
        var topicName = "rtTestTopicOffline";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
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

        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        await using var rt = await client.GetReliableTopicAsync<int>(topicName);

        var mne = new ManualResetEvent(false);

        await rt.SubscribeAsync(events =>
            events.Message((sender, args) => { mne.Set(); }));

        await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(1)", Lang.JAVASCRIPT);

        Assert.True(await mne.WaitOneAsync());

        mne.Reset();

        await RestartCluster(async () =>
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 15_000, 100));

        // Last push was 1, continue from 2.
        for (int i = 2; i < 20; i++)
            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(" + i + ")", Lang.JAVASCRIPT);

        Assert.True(await mne.WaitOneAsync());
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(80_000)]
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
            new ReliableTopicEventHandlerOptions() {IsLossTolerant = true});

        await RestartCluster(async () =>
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 15_000, 100));

        await rt2.PublishAsync(1);

        Assert.True(await mne.WaitOneAsync());
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(80_000)]
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

        var receivedCount = 0;

        var sId = await rt.SubscribeAsync(events =>
                events.Message((sender, args) =>
                {
                    Console.WriteLine("Message:" + args.Payload);
                    Interlocked.Increment(ref receivedCount);
                }),
            new ReliableTopicEventHandlerOptions() {IsLossTolerant = true});

        await RestartCluster(async () =>
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 15_000, 100));

        await AssertEx.SucceedsEventually(async () =>
        {
            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(99)", Lang.JAVASCRIPT);
            Assert.Greater(receivedCount, 1);
        }, 10_000, 100);

        var rtImpl = (HReliableTopic<int>) rt;
        Assert.True(rtImpl.TryGetExecutor(sId, out var exc));
        Assert.False(exc.IsDisposed);
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(80_000)]
    public async Task TestClusterRestartWhenNotLossTolerant()
    {
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

        var m = 0;
        for (int i = 0; i < 100; i++)
            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(" + m++ + ")", Lang.JAVASCRIPT);

        var msgReceivedCount = 0;

        var rtEventOpt = new ReliableTopicEventHandlerOptions() {IsLossTolerant = false, InitialSequence = 0, StoreSequence = true};

        var sId = await rt.SubscribeAsync(events =>
            events.Message(async (sender, args) =>
            {
                await Task.Delay(500); //slow down the consumer.
                Console.WriteLine("SEQ:" + args.Sequence);
                Console.WriteLine("Message:" + args.Payload);
                Interlocked.Increment(ref msgReceivedCount);
            }), rtEventOpt);

        // Let subscriber receives the sequence before restart.
        await Task.Delay(500);

        await RestartCluster(async () =>
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 15_000, 100));

        // Cluster restarted, sequence and data should be lost. The subscriber cannot continue since it is not loss tolerant.
        for (int i = 0; i < 100; i++)
            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(" + m + ")", Lang.JAVASCRIPT);

        await AssertEx.SucceedsEventually(async () =>
        {
            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(" + m + ")", Lang.JAVASCRIPT);
            m++;
            Assert.False(rt.IsSubscription(sId));
        }, 15_000, 100);
        
        Assert.AreEqual(0, msgReceivedCount);
        await rt.DestroyAsync();
    }

    [Test]
    [Timeout(120_000)]
    public async Task TestReliableTopicUnderStress()
    {
        var topicName = "rtTestTopicStress";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Information)
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

        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<int>(topicName);

        int c1Received = 0, c2Received = 0;

        var c1 = await rt.SubscribeAsync(events =>
            events.Message((sender, args) =>
            {
                Assert.AreEqual(c1Received, args.Payload);
                Interlocked.Increment(ref c1Received);
                if (c1Received % 10000 == 0) Console.WriteLine($"C1 Received: {c1Received}");
            }));

        var c2 = await rt.SubscribeAsync(events =>
            events.Message((sender, args) =>
            {
                Assert.AreEqual(c2Received, args.Payload);
                Interlocked.Increment(ref c2Received);
                if (c2Received % 10000 == 0) Console.WriteLine($"C2 Received: {c2Received}");
            }));


        var cancelToken = new CancellationTokenSource();
        cancelToken.CancelAfter(30_000);

        await Task.Delay(1_000);

        var producer = Task.Run(async () =>
        {
            var send = 0;
            while (!cancelToken.IsCancellationRequested)
                await rt.PublishAsync(send++, cancelToken.Token);

            return send;
        }, cancelToken.Token);

        var totalSend = await producer;

        await AssertEx.SucceedsEventually(() =>
        {
            Assert.AreEqual(totalSend, c1Received);
            Assert.AreEqual(totalSend, c2Received);
        }, 60_000, 200);


        Assert.True(await rt.UnsubscribeAsync(c1));
        Assert.True(await rt.UnsubscribeAsync(c2));
        Assert.False(await rt.UnsubscribeAsync(c2));
        Assert.False(rt.IsSubscription(c1));
        Assert.False(rt.IsSubscription(c2));
        await rt.DestroyAsync();
    }
}
