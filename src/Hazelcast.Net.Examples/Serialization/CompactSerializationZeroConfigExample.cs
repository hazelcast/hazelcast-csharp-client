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
using System.Threading.Tasks;

namespace Hazelcast.Examples.Serialization;

// ReSharper disable once UnusedMember.Global
public class CompactSerializationZeroConfigExample
{
    public static async Task Main(string[] args)
    {
        // do *not* configure anything special re. serialization - persons will be serialized
        // in zero-config mode via a reflection-based serializer, which serializes every
        // publicly gettable & settable properties.

        // create options
        var options = new HazelcastOptionsBuilder()
            .With(args)
            .WithConsoleLogger()
            .Build();

        // create an Hazelcast client and connect to a server running on localhost
        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

        // get a map of persons
        await using var personMap = await client.GetMapAsync<int, Person>("person-map");

        // create a person
        var person = new Person
        {
            Name = "John",
            Surname = "Doe",
            DateOfBirth = DateTime.Today
        };

        // store the person into the map
        await personMap.SetAsync(1, person);

        // get it back
        var person2 = await personMap.GetAsync(1);

        // same!
        Console.WriteLine(person);
        Console.WriteLine(person2);
    }

    public class Person
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public DateTime DateOfBirth { get; set; }

        public override string ToString()
            => $"Person(Name='{Name}', Surname='{Surname}', DateOfBirth='{DateOfBirth:d}'";
    }
}