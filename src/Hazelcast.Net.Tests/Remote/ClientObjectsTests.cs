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
using System.Threading.Tasks;
using Hazelcast.Models;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NuGet.Versioning;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote;

[TestFixture]
internal class ClientObjectsTests : SingleMemberClientRemoteTestBase 
{
    [Test]
    [ServerCondition("[5.0,)")] // 5.0 and above
    public async Task CanGetDistributedObjects()
    {
        var map = await Client.GetMapAsync<int, int>(CreateUniqueName());
        var list = await Client.GetListAsync<int>(CreateUniqueName());
        var objects = await Client.GetDistributedObjectsAsync();

        var expectedCount = ServerVersion.GetVersion() >= new NuGetVersion(5, 2, 0)
            ? 2 // the map and the list
            : 3; // and also the internal sql catalog

        Assert.That(objects.Count, Is.EqualTo(expectedCount));
        Assert.That(objects, Does.Contain(new DistributedObjectInfo(map.ServiceName, map.Name)));
        Assert.That(objects, Does.Contain(new DistributedObjectInfo(list.ServiceName, list.Name)));

        await using var client2 = await HazelcastClientFactory.StartNewClientAsync(Client.Options);
        objects = await client2.GetDistributedObjectsAsync();
        Assert.That(objects.Count, Is.EqualTo(expectedCount));
        Assert.That(objects, Does.Contain(new DistributedObjectInfo(map.ServiceName, map.Name)));
        Assert.That(objects, Does.Contain(new DistributedObjectInfo(list.ServiceName, list.Name)));
    }
}