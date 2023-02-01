// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Networking;
using Hazelcast.Partitioning;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact;

[TestFixture]
public class CompactQaTests : ClusterRemoteTestBase
{
    private IDisposable UseHConsole() => HConsole.Capture(o => o
        .Configure().SetMaxLevel()
        .Configure(this).SetPrefix("TEST")
        .Configure<SocketConnectionBase>().SetIndent(8).SetPrefix("SOCKET").SetLevel(0)
        .Configure<ClientMessageConnection>().SetMinLevel()
        .Configure<AsyncContext>().SetMinLevel()
        .Configure<Partitioner>().SetLevel(1));

    [Test]
    public async Task ClientGoesOffline()
    {
        using var _ = UseHConsole();

        var throwException = false;

        // start a member
        var member = await RcClient.StartMemberAsync(RcCluster).CfAwait();
        await using var cleanup = new DisposeAsyncAction(async () => await RcClient.StopMemberAsync(RcCluster, member));

        // use a clean client + hook into cluster messaging to capture messages (before client starts)
        await using var client = HazelcastClientFactory.CreateClient(CreateHazelcastOptions());
        // use an internal-level handler, exceptions in user-level handlers are caught
        client.Cluster.Connections.ConnectionOpened += (_, _, _, _) =>
        {
            if (throwException) throw new Exception("bang!");
            return default;
        };
        await client.StartAsync(CancellationToken.None);

        var map = await client.GetMapAsync<int, IGenericRecord>("bar");
        await map.PutAsync(1, GenericRecordBuilder.Compact("bar1").Build());

        // stop the member, wait until it is actually removed (else we might reconnect to it)
        await RcClient.StopMemberWaitRemovedAsync(client, RcCluster, member).CfAwait();

        // trigger exceptions
        throwException = true;

        // start another member
        member = await RcClient.StartMemberAsync(RcCluster).CfAwait();

        // this will never complete because we'll never be able to reconnect
        //
        // Java: ReconnectMode can be OFF, ON (blocking invocations) or ASYNC (not blocking, triggers HazelcastClientOfflineException)
        // .NET: ReconnectMode can be DoNotReconnect = OFF, ReconnectSync or ReconnectAsync - but these two have the same effect
        //
        // In Java, ASYNC causes any invocation to *immediately* fail with HazelcastClientOfflineException if the client is
        // reconnecting, whereas ON causes the invocation to be retried, and it may eventually fail with OperationTimeoutException.
        //
        // In .NET, invocations are tried (and retried) while the client is reconnecting, until either the client reconnects and
        // the invocation succeeds, or it times out. So, essentially, .NET is ON and it makes sense that we get a TaskTimeoutException
        // below.

        await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await map.PutAsync(2, GenericRecordBuilder.Compact("bar2").Build()).CfAwait());

        await RcClient.StopMemberAsync(RcCluster, member).CfAwait();
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
        client.Cluster.Messaging.SendingMessage += message =>
        {
            messages.Add(message);
            return default;
        };
        await client.StartAsync(CancellationToken.None);

        var map = await client.GetMapAsync<int, IGenericRecord>("bar");

        // add values = will publish the corresponding schemas
        await map.PutAsync(1, GenericRecordBuilder.Compact("bar1").Build());
        await map.PutAsync(2, GenericRecordBuilder.Compact("bar2").Build());

        // stop the member, wait until it is actually removed (else we might reconnect to it)
        await RcClient.StopMemberWaitRemovedAsync(client, RcCluster, member);

        messages.Clear();

        // start another member
        member = await RcClient.StartMemberAsync(RcCluster);

        // new member, values are lost
        var value1 = await map.GetAsync(1);
        Assert.That(value1, Is.Null);

        // if we've been reconnected, schemas have been republished
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