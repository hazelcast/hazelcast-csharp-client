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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Remote;
using Hazelcast.Testing.TestServer;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote;

public class ClientReliableTopicTests : SingleMemberRemoteTestBase
{
    private IDisposable HConsoleForTest()
        => HConsole.Capture(options => options
            .Configure().SetMaxLevel()
            .Configure(this).SetMaxLevel().SetPrefix("TEST"));

    internal string ClusterName => "reliableTopicCluster";

    [Test]
    [Timeout(20_000)]
    public async Task TestReliableTopic([Values] TopicOverloadPolicy policy)
    {
        //HConsoleForTest();
        var topicName = "rtTestTopic";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(policy, 3);
                opt.ClusterName = RcCluster.Id;

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
    public async Task TestClusterRestartWhenSubscribed()
    {
        HConsoleForTest();
        var topicName = "rtTestTopic2";

        var (cluster, member) = await CreateSimpleCluster();

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ClusterName = cluster.Id;
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

        await RcClient.StopMemberWaitClosedAsync(client, cluster, member);

        await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 5_000, 100);

        member = await RcClient.StartMemberAsync(cluster.Id);

        await rt2.PublishAsync(1);

        Assert.True(await mne.WaitOneAsync());
        await RcClient.StopMemberAsync(cluster, member);
    }

    [Test]
    [Timeout(60_000)]
    public async Task TestClusterRestartAfterInvocationTimeOut()
    {
        HConsoleForTest();
        var topicName = "rtTestTopic3";
        var timeOut = 2_000;
        var (cluster, member) = await CreateSimpleCluster();

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ClusterName = cluster.Id;
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

        await RcClient.StopMemberWaitClosedAsync(client, cluster, member);

        await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 5_000, 100);

        await Task.Delay(timeOut);

        member = await RcClient.StartMemberAsync(cluster.Id);

        await RcClient.ExecuteOnControllerAsync(cluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(99)", Lang.JAVASCRIPT);

        Assert.True(await mne.WaitOneAsync());
        var rtImpl = (HReliableTopic<int>) rt;
        Assert.True(rtImpl.TryGetExecutor(sId, out var exc));
        Assert.False(exc.IsDisposed);

        await RcClient.StopMemberAsync(cluster, member);
    }

    [Test]
    [Timeout(60_000)]
    public async Task TestClusterRestartWhenNotLossTolerant()
    {
        //HConsoleForTest();
        var topicName = "rtNotTolerant";

        var (cluster, member) = await CreateSimpleCluster();

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With((conf, opt) =>
            {
                opt.ClusterName = cluster.Id;
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
            await RcClient.ExecuteOnControllerAsync(cluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(" + i + ")", Lang.JAVASCRIPT);

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

        // Let subscriber receive the sequence before restart.
        await Task.Delay(500);

        await RcClient.ShutdownClusterAsync(cluster.Id);
        await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Disconnected, client.State); }, 5_000, 100);
        (cluster, member) = await CreateSimpleCluster();

        // Cluster restarted, sequence and data should are lost. The subscriber cannot continue since it is not loss tolerant.
        for (int i = 0; i < 100; i++)
        {
            await RcClient.ExecuteOnControllerAsync(cluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(" + (i + 100) + ")", Lang.JAVASCRIPT);
        }

        await AssertEx.SucceedsEventually(async () =>
        {
            await RcClient.ExecuteOnControllerAsync(cluster.Id, "instance_0.getReliableTopic(\"" + topicName + "\").publish(101)", Lang.JAVASCRIPT);
            Assert.False(rt.IsSubscriptionExist(sId));
        }, 10_000, 100);

        Assert.AreEqual(0, msgReceivedCount);

        var rtImpl = (HReliableTopic<int>) rt;
        Assert.AreEqual(0, msgReceivedCount);
        await RcClient.StopMemberAsync(cluster, member);
    }

    private async Task<(Cluster, Member)> CreateSimpleCluster()
    {
        var cluster = await RcClient.CreateClusterKeepClusterNameAsync(ServerVersion.DefaultVersion.Version.ToString(), Resources.Cluster_ReliableTopic);
        var member = await RcClient.StartMemberAsync(cluster.Id);
        return (cluster, member);
    }
}
