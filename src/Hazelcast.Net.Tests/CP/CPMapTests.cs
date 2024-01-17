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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.DistributedObjects;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.CP;

[Category("enterprise")]
public class CPMapTests : MultiMembersRemoteTestBase
{
    private string _defaultMapName = "myMap";
    private string _groupName = "group1";

    private List<Member> _members = new();
    public IHazelcastClient Client { get; private set; }
    protected override string RcClusterConfiguration => TestFiles.ReadAllText(this, "Cluster/cp.xml");


    [OneTimeSetUp]
    public async Task TestOneTimeSetUp()
    {
        // CP-subsystem wants at least 3 members
        for (var i = 0; i < 3; i++) await AddMember().CfAwait();
        Client = await CreateAndStartClientAsync().CfAwait();
    }


    [Test]
    public async Task TestGetCPMapWithNullName()
    {
        Assert.ThrowsAsync<ArgumentException>(() => Client.CPSubsystem.GetMapAsync<int, int>(null));
    }

    [Test]
    public async Task TestGetCPMap()
    {
        var map = await Client.CPSubsystem.GetMapAsync<int, int>(_defaultMapName);
        Assert.NotNull(map);
        Assert.IsInstanceOf<CPMap<int, int>>(map);
        Assert.IsInstanceOf<ICPDistributedObject>(map);

        var doBase = (CPDistributedObjectBase) map;
        Assert.AreEqual(ServiceNames.CPMap, doBase.ServiceName);
        Assert.AreEqual(_defaultMapName, doBase.Name);
        Assert.IsNull(doBase.PartitionKey);
        
        Assert.AreEqual(CPSubsystem.DefaultGroupName, map.GroupId.Name);
    }

    [Test]
    public async Task TestPut()
    {
        var map = await Client.CPSubsystem.GetMapAsync<int, int>(_defaultMapName);
        var (key, val) = (101, 1001);

        // Attention: Neither key nor value can be null.

        var prevVal = await map.PutAsync(key, val)!;
        Assert.AreEqual(default(int), prevVal);
        Assert.That(prevVal, Is.Zero);
        prevVal = await map.PutAsync(key, val)!;
        Assert.AreEqual(val, prevVal);
    }

    [Test]
    public async Task TestSet()
    {
        var map = await Client.CPSubsystem.GetMapAsync<int, int>(_defaultMapName);
        var (key, val) = (102, 1002);

        // Attention: Neither key nor value can be null. It's ok to suppress.
        await map.SetAsync(key, val);
        var setVal = await map.GetAsync(key)!;
        Assert.AreEqual(val, setVal);
    }

    [Test]
    public async Task TestGet()
    {
        var map = await Client.CPSubsystem.GetMapAsync<int, int>(_defaultMapName);
        var (key, val) = (103, 1003);

        var setVal = await map.GetAsync(key)!;
        Assert.AreEqual(default(int), setVal);

        await map.SetAsync(key, val);

        setVal = await map.GetAsync(key)!;
        Assert.AreEqual(val, setVal);
    }

    [Test]
    public async Task TestRemove()
    {
        var map = await Client.CPSubsystem.GetMapAsync<int, int>(_defaultMapName);
        var (key, val) = (104, 1004);

        var setVal = await map.RemoveAsync(key)!;
        Assert.AreEqual(default(int), setVal);

        await map.SetAsync(key, val);

        setVal = await map.RemoveAsync(key)!;
        Assert.AreEqual(val, setVal);
    }

    [Test]
    public async Task TestDelete()
    {
        var map = await Client.CPSubsystem.GetMapAsync<int, int>(_defaultMapName);
        var (key, val) = (105, 1005);

        // Nothing happens.
        await map.DeleteAsync(key);

        await map.SetAsync(key, val);
        await map.DeleteAsync(key);
        var getVal = await map.GetAsync(key)!;
        Assert.AreEqual(default(int), getVal);
    }

    [Test]
    public async Task TestCompareAndSet()
    {
        var map = await Client.CPSubsystem.GetMapAsync<string, string>("stringMap");

        var key = "myKey";
        var val1 = "val1";
        var val2 = "val2";

        await map.SetAsync(key, val1);

        var isSet = await map.CompareAndSetAsync(key, "randomKey", "randomValue");
        Assert.False(isSet);
        var actual = await map.GetAsync(key)!;
        Assert.AreEqual(val1, actual);

        isSet = await map.CompareAndSetAsync(key, val1, val2);
        Assert.True(isSet);
        actual = await map.GetAsync(key)!;
        Assert.AreEqual(val2, actual);
    }

    [Test]
    public async Task TestTryUseAfterDestroy()
    {
        var mapName = "myMap1";
        var map = await Client.CPSubsystem.GetMapAsync<int, int>(mapName);
        await map.DestroyAsync();
        var ex = Assert.ThrowsAsync<RemoteException>(() => map.SetAsync(1, 1));
        Assert.AreEqual(RemoteError.DistributedObjectDestroyed, ex!.Error);
    }

    [Test]
    public async Task TestTryCreateAfterDestroy()
    {
        var mapName = "myMap2";
        var map = await Client.CPSubsystem.GetMapAsync<int, int>(mapName);
        await map.DestroyAsync();
        var ex = Assert.ThrowsAsync<RemoteException>(async () =>
        {
            var mapNew = await Client.CPSubsystem.GetMapAsync<int, int>(mapName);
            await mapNew.SetAsync(1, 1);
        });

        Assert.AreEqual(RemoteError.DistributedObjectDestroyed, ex!.Error);
    }
}
