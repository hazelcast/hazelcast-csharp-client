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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Configuration.Dynamic;

[TestFixture]
public class DynamicConfigureRingBufferTests : DynamicConfigureTestBase
{
    [Test]
    public async Task CanConfigureEverything()
    {
        var options = CreateHazelcastOptions();
        options.ClusterName = RcCluster.Id;
        await using var client = await HazelcastClientFactory.StartNewClientAsync(options).CfAwait();

        await client.DynamicOptions.ConfigureRingbufferAsync("buffer-name", options =>
        {
            options.Name = "buffer-name";
            options.AsyncBackupCount = 1;
            options.BackupCount = 1;
            options.Capacity = 1;
            options.InMemoryFormat = InMemoryFormat.Binary;
            options.MergePolicy.BatchSize = 1;
            options.MergePolicy.Policy = "policy";
            options.TimeToLiveSeconds = 1;
            options.SplitBrainProtectionName = "splitBrainProtectionName";
            options.RingbufferStore.Enabled = true;
            options.RingbufferStore.ClassName = "classNam";
            options.RingbufferStore.FactoryClassName = "factoryClassName";
        });
    }

    [Test]
    [ServerCondition("[5.4]")]
    public async Task DefaultOptionsEncodeToSameMessageAsJava()
    {
        // CI error: trying to invoke this:
        // com.hazelcast.client.impl.protocol.codec.DynamicConfigAddRingbufferConfigCodec.encodeRequest(
        //   String,
        //   int,int,int,int,
        //   String,
        //   RingbufferStoreConfigHolder,
        //   String,String,
        //   int,
        //   String)]
        //
        // this what we have in Java codebase:
        // encodeRequest(
        //   java.lang.String name,
        //   int capacity, int backupCount, int asyncBackupCount, int timeToLiveSeconds,
        //   java.lang.String inMemoryFormat,
        //   @Nullable com.hazelcast.client.impl.protocol.task.dynamicconfig.RingbufferStoreConfigHolder ringbufferStoreConfig,
        //   @Nullable java.lang.String splitBrainProtectionName, java.lang.String mergePolicy,
        //   int mergeBatchSize) {
        //
        // now what is the trailing string?!
        //
        // in hazelcast-repo 5.4 it's
        // encodeRequest(..., @Nullable java.lang.String namespace) {

        const string script = @"

var serializationService = instance_0.serializationService

var RingbufferConfig = Java.type(""com.hazelcast.config.RingbufferConfig"")
var ringbufferConfig = new RingbufferConfig(""buffer-name"")

var RingbufferStoreConfigHolder = Java.type(""com.hazelcast.client.impl.protocol.task.dynamicconfig.RingbufferStoreConfigHolder"")
var ringbufferStoreConfig = ringbufferConfig.getRingbufferStoreConfig() != null &&
                            ringbufferConfig.getRingbufferStoreConfig().isEnabled()
    ? RingbufferStoreConfigHolder.of(ringbufferConfig.getRingbufferStoreConfig(), serializationService)
    : null

var DynamicConfigAddRingbufferConfigCodec = Java.type(""com.hazelcast.client.impl.protocol.codec.DynamicConfigAddRingbufferConfigCodec"")
var message = DynamicConfigAddRingbufferConfigCodec.encodeRequest(
    ringbufferConfig.getName(),
    ringbufferConfig.getCapacity(), ringbufferConfig.getBackupCount(),
    ringbufferConfig.getAsyncBackupCount(), ringbufferConfig.getTimeToLiveSeconds(),
    ringbufferConfig.getInMemoryFormat().name(), 
    ringbufferStoreConfig,
    ringbufferConfig.getSplitBrainProtectionName(), ringbufferConfig.getMergePolicyConfig().getPolicy(),
    ringbufferConfig.getMergePolicyConfig().getBatchSize(),
    null /*namespace*/)

" + ResultIsJavaMessageBytes;

        var javaBytes = await ScriptToBytes(script);

        var options = new RingbufferOptions("buffer-name");
        var message = DynamicConfigAddRingbufferConfigCodec.EncodeRequest(
            options.Name,
            options.Capacity,
            options.BackupCount,
            options.AsyncBackupCount,
            options.TimeToLiveSeconds,
            options.InMemoryFormat.ToJavaString(),
            options.RingbufferStore is { Enabled: true } ? RingbufferStoreConfigHolder.Of(options.RingbufferStore) : null,
            options.SplitBrainProtectionName,
            options.MergePolicy.Policy,
            options.MergePolicy.BatchSize,
            null /*namespace*/
        );

        var dotnetBytes = MessageToBytes(message);

        AssertMessagesAreIdentical(javaBytes, dotnetBytes);
    }
}
