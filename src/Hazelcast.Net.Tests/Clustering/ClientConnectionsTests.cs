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

using System.Reflection;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Partitioning;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering;

[TestFixture]
public class ClientConnectionsTests
{
    [Test]
    public async Task ConnectMemberCoverage()
    {
        // this test validates that the ConnectMembers task of ClientConnections can complete nicely
        // without being hard-canceled - this is required to get a stable test coverage (as it may
        // or may not happen in other tests, depending on timing)

        var loggerFactory = new NullLoggerFactory();
        var options = new HazelcastOptions();
        var serializationService = new SerializationServiceBuilder(loggerFactory).Build();
        var clusterState = new ClusterState(options, "cluster-name", "client-name", new Partitioner(), loggerFactory);
        var clusterMembers = new ClusterMembers(clusterState, new TerminateConnections(loggerFactory));
        var clusterConnections = new ClusterConnections(clusterState, clusterMembers, serializationService);

        var connectMembersField = typeof (ClusterConnections).GetField("_connectMembers", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(connectMembersField, Is.Not.Null);
        var connectMembers = (Task) connectMembersField.GetValue(clusterConnections);

        // and now, the ConnectMembers background task should be running
        // now, we want to stop it without throwing - just nicely exiting its foreach loop
        // this means closing the MemberConnectionQueue, which is owned by ClusterMembers

        // disposing ClusterMembers is going to close the queue indeed
        await clusterMembers.DisposeAsync();

        // and the task completes nicely
        Assert.That(connectMembers.Status, Is.EqualTo(TaskStatus.RanToCompletion));
    }
}