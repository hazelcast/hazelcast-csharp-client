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
using Hazelcast.Models;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote;

public class ClientReliableTopicTests : SingleMemberRemoteTestBase
{
    private IDisposable HConsoleForTest()
        => HConsole.Capture(options => options
            .Configure().SetMinLevel()
            .Configure(this).SetMaxLevel().SetPrefix("TEST"));

    [Test]
    [Timeout(20_000)]
    public async Task TestReliableTopic([Values] TopicOverloadPolicy policy)
    {
        HConsoleForTest();
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
}
