// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Query;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Support;

[TestFixture]
[ServerCondition("[5.2,)")]
public class Issue868 : SingleMemberClientRemoteTestBase
{
    internal class Thing
    {
        // need a parameter-less constructor
        public Thing()
        { }

        public Thing(string name)
        {
            Name = name;
        }

        // need to be publicly settable
        public string Name { get; set; }

        public override string ToString()
            => $"Thing {{ Name=\"{Name}\" }}";
    }

    [Test]
    public async Task Test()
    {
        // the Thing class will be serialized with compact serialization, no better alternative
        await using var map = await Client.GetMapAsync<int, Thing>(CreateUniqueName());
        for (var i = 0; i < 10; i++) await map.SetAsync(i, new Thing(i.ToString()));

        var schemas0 = Client
            .MustBe<HazelcastClient>().SerializationService.CompactSerializer.Schemas
            .MustBe<Schemas>().SchemaCache;

        // just to be sure: a schema has been created indeed
        Assert.That(schemas0.Count, Is.EqualTo(1));
        var schema = schemas0.First().Value;
        Assert.That(schema.Schema.TypeName, Is.EqualTo("Hazelcast.Tests.Support.Issue868+Thing, Hazelcast.Net.Tests"));

        // the new client does not know about the Thing compact schema, yet
        await using var client1 = await HazelcastClientFactory.StartNewClientAsync(CreateHazelcastOptions());

        var schemas1 = client1
            .MustBe<HazelcastClient>().SerializationService.CompactSerializer.Schemas
            .MustBe<Schemas>().SchemaCache;

        // just to be sure: no schema so far
        Assert.That(schemas1.Count, Is.EqualTo(0));

        // go fetch entries
        // reproduced: this indeed throws because the schema is not known during UpdateAnchors
        // fixed: GetEntriesAsync modified, now works
        // FIXME can we be sure that *all* anchors are going to be part of the result?
        // what's an anchor really?
        // node client says that anchor is 'the last value object on the previous page'
        var predicate = Predicates.Page(2);
        var entries = await map.GetEntriesAsync(predicate);
        Assert.That(entries.Count, Is.EqualTo(2));

        var n = 0;
        foreach (var (key, value) in entries)
            Console.WriteLine($"RESULT [{n++}] {key}: {value}");

        foreach (var (pageNo, (key, value)) in predicate.MustBe<PagingPredicate>().AnchorList)
            Console.WriteLine($"ANCHOR [{pageNo}] {key}: {value}");

        // moving on to the next page... 
        // result contains id 2 and 3, anchors are 1 and 3
        // => there *is* an anchor that is not a value
        predicate.NextPage();
        entries = await map.GetEntriesAsync(predicate);
        Assert.That(entries.Count, Is.EqualTo(2));

        n = 0;
        foreach (var (key, value) in entries)
            Console.WriteLine($"RESULT [{n++}] {key}: {value}");

        foreach (var (pageNo, (key, value)) in predicate.MustBe<PagingPredicate>().AnchorList)
            Console.WriteLine($"ANCHOR [{pageNo}] {key}: {value}");
    }
}