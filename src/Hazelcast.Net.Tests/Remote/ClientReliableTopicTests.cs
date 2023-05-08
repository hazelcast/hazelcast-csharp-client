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
using Hazelcast.Models;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Microsoft.Extensions.Logging.Console;

namespace Hazelcast.Tests.Remote;

public class ClientReliableTopicTests : SingleMemberRemoteTestBase
{
    [Test]
    [Timeout(30_000)]
    public async Task TestReliableTopic([Values] TopicOverloadPolicy policy)
    {
        var topicName = "rtTestTopic1";

        var options = new HazelcastOptionsBuilder()
            .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Debug)
            .With(opt =>
            {
                opt.ReliableTopics[topicName] = new ReliableTopicOptions(policy, 2);
                opt.ClusterName = RcCluster.Id;
                
                opt.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            })
            .Build();


        var client = await HazelcastClientFactory.StartNewClientAsync(options);
        var rt = await client.GetReliableTopicAsync<string>(topicName);
        var msgCount = 5;
        var sSlim = new SemaphoreSlim(msgCount);
        
        await rt.SubscribeAsync(events => events.Message((sender, args) =>
        {
            Assert.AreEqual("msg" + sSlim.CurrentCount, args.Payload);
            Assert.AreEqual(sSlim.CurrentCount, args.Sequence);
            Console.WriteLine("SEQ:"+args.Sequence);
            sSlim.Release(1);
        }));

        

        for (var i = 1; i <= msgCount; i++)
        {
            await rt.PublishAsync("msg" + i);
            await Task.Delay(10);
        }

        await sSlim.WaitAsync();
        Assert.AreEqual(0, sSlim.CurrentCount);
    }
}
