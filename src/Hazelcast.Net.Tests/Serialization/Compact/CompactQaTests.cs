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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Partitioning;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact;

[TestFixture]
[ServerCondition("[5.2,)")]
public class CompactQaTests : ClusterRemoteTestBase
{
    [Test]
    // [Explicit("See comment in test.")]
    public async Task MemberAddressMatch()
    {
        HConsole.Configure(options => options.ConfigureDefaults(this));

        var member = await RcClient.StartMemberAsync(RcCluster).CfAwait();
        await using var cleanup = new DisposeAsyncAction(async () => await RcClient.StopMemberAsync(RcCluster, member));

        var options = CreateHazelcastOptions();
        options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10_000;
        options.Networking.Addresses.Clear();
        options.Networking.Addresses.Add("127.0.0.2:5701");

        // this relies on the connect queue running all the time, but we have now disabled
        // the queue when disconnected - so this test cannot work anymore, we will need to
        // find a different way to do address re-routing in the future, if other clients
        // do it too.

        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
    }

    [Test]
    public async Task ExceptionPreventsClientFromReconnecting()
    {
        HConsole.Configure(options => options.ConfigureDefaults(this));

        // in case a test times out, NUnit just reports that it failed, without further
        // details - in fact, it does not even say that the test timed out - so by wrapping
        // the actual test method this way, we make sure that the test "fails" instead of
        // timing out, and we get proper output (eg HConsole output)
        await ExceptionPreventsClientFromReconnectingTask().CfAwait(TimeSpan.FromSeconds(60));
    }

    private async Task ExceptionPreventsClientFromReconnectingTask()
    {
        var throwException = false;

        // start a member
        var member = await RcClient.StartMemberAsync(RcCluster).CfAwait();
        await using var cleanup = new DisposeAsyncAction(async () => await RcClient.StopMemberAsync(RcCluster, member));

        // use a clean client + hook into cluster messaging to capture messages (before client starts)
        var options = CreateHazelcastOptions();
        options.Messaging.RetryTimeoutSeconds = 20;
        await using HazelcastClient client = HazelcastClientFactory.CreateClient(options);
        var conQueue = (MemberConnectionQueue) client.Cluster.Members.MemberConnectionRequests;
        await client.StartAsync(CancellationToken.None);

        var map = await client.GetMapAsync<int, IGenericRecord>("bar");
        await map.PutAsync(1, GenericRecordBuilder.Compact("bar1").Build());

        HConsole.WriteLine(this, "-------- STOP MEMBER --------");

        // stop the member, wait until it is actually removed (else we might reconnect to it)
        await RcClient.StopMemberWaitRemovedAsync(client, RcCluster, member).CfAwait();

        // trigger exceptions
        Volatile.Write(ref throwException, true);
        await conQueue.SuspendAsync();


        async Task RecoverAfter(int delayMilliseconds)
        {
            await Task.Delay(delayMilliseconds);
            Volatile.Write(ref throwException, false);
            conQueue.Resume();
        }

        var recoverTask = RecoverAfter(5000); // fire-and-forget


        HConsole.WriteLine(this, "-------- START MEMBER --------");

        // start another member
        member = await RcClient.StartMemberAsync(RcCluster).CfAwait();

        HConsole.WriteLine(this, "-------- PUT ASYNC --------");


        // this will eventually complete once we're able to reconnect
        await map.PutAsync(2, GenericRecordBuilder.Compact("bar2").Build()).CfAwait();


        HConsole.WriteLine(this, "-------- STOP MEMBER --------");

        await RcClient.StopMemberAsync(RcCluster, member).CfAwait();

        HConsole.WriteLine(this, "-------- END --------");
    }

    [Test]
    public async Task ClusterRestart()
    {
        //using var _ = UseHConsole();

        // start a member
        var member = await RcClient.StartMemberAsync(RcCluster).CfAwait();
        await using var cleanup = new DisposeAsyncAction(async () => await RcClient.StopMemberAsync(RcCluster, member));

        var messages = new List<ClientMessage>();

        // use a clean client + hook into cluster messaging to capture messages (before client starts)
        await using var client = HazelcastClientFactory.CreateClient(CreateHazelcastOptions());
        client.Cluster.Messaging.SendingMessage += (message, _) =>
        {
            messages.Add(message);
            return default;
        };
        await client.StartAsync(CancellationToken.None);

        var clientMembers = client.Members;
        Assert.That(clientMembers.Count, Is.EqualTo(1));
        Assert.That(clientMembers.First().Member.Id, Is.EqualTo(Guid.Parse(member.Uuid)));
        HConsole.WriteLine(this, $"CONNECTED TO MEMBER {member.Uuid}");

        var map = await client.GetMapAsync<int, IGenericRecord>("bar");

        // add values = will publish the corresponding schemas
        await map.PutAsync(1, GenericRecordBuilder.Compact("bar1").Build());
        await map.PutAsync(2, GenericRecordBuilder.Compact("bar2").Build());

        // stop the member, wait until it is actually removed (else we might reconnect to it)
        await RcClient.StopMemberWaitRemovedAsync(client, RcCluster, member);

        messages.Clear();

        // start another member
        member = await RcClient.StartMemberAsync(RcCluster);

        // ensure that, eventually, the client is going to connect to *that* other member
        // and not, because of some timing issues, on the previous one that would not stop
        // fast enough - StopMemberWaitRemoved waits for the client to lose its connection
        // to the member but the remote controller has no wait of notifying us that the
        // member is actually dead for real and not going to accept connections anymore.
        await AssertEx.SucceedsEventually(() =>
        {
            clientMembers = client.Members;
            Assert.That(clientMembers.Count, Is.EqualTo(1));
            Assert.That(clientMembers.First().Member.Id, Is.EqualTo(Guid.Parse(member.Uuid)));
        }, 60_000, 1_000);

        // new member, values are lost
        var value1 = await map.GetAsync(1);
        Assert.That(value1, Is.Null);

        // yet we've been reconnected, schemas have been republished
        Assert.That(messages.Any(x => x.MessageType == ClientSendAllSchemasCodec.RequestMessageType));

        await RcClient.StopMemberAsync(RcCluster, member).CfAwait();
    }

    [Test]
    public async Task ReadSchemaAfterWrite_withObjectValueType()
    {
        //using var _ = UseHConsole();

        // start a member
        var member = await RcClient.StartMemberAsync(RcCluster).CfAwait();
        await using var cleanup = new DisposeAsyncAction(async () => await RcClient.StopMemberAsync(RcCluster, member));

        // use a clean client
        await using var client = await CreateAndStartClientAsync();

        var map = await client.GetMapAsync<int, object>("bar");

        await map.PutAsync(1, GenericRecordBuilder.Compact("bar1").Build());
        var value = await map.PutAsync(1, GenericRecordBuilder.Compact("bar2").Build());
        Assert.That(value, Is.Not.Null);
        Assert.That(value, Is.InstanceOf<IGenericRecord>());

        await RcClient.StopMemberAsync(RcCluster, member).CfAwait();
    }

    protected override HazelcastOptions CreateHazelcastOptions()
    {
        var options = base.CreateHazelcastOptions();
        options.Networking.ReconnectMode = ReconnectMode.ReconnectSync;
        options.Messaging.RetryTimeoutSeconds = 10;
        return options;
    }

    protected override HazelcastOptionsBuilder CreateHazelcastOptionsBuilder()
    {
        return base.CreateHazelcastOptionsBuilder().WithHConsoleLogger();
    }
}
