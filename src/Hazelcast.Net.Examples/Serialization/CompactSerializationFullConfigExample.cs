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
using System.Threading.Tasks;
using Hazelcast.Models;
using Hazelcast.Serialization.Compact;

namespace Hazelcast.Examples.Serialization;

// ReSharper disable once UnusedMember.Global
public class CompactSerializationFullConfigExample
{
    public static async Task Main(string[] args)
    {
        // create options
        var options = new HazelcastOptionsBuilder()
            .With(args)
            .WithConsoleLogger()
            .Build();

        // explicitly provide a serializer for the Person class
        options.Serialization.Compact.AddSerializer(new PersonCompactSerializer());

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

    public class PersonCompactSerializer : CompactSerializerBase<Person>
    {
        // provide the type-name that identifies the class serialization schema
        public override string TypeName => "person";

        // de-serialize
        public override Person Read(ICompactReader reader)
        {
            return new Person
            {
                Name = reader.ReadString("name"),
                Surname = reader.ReadString("surname"),

                // ReSharper disable once PossibleInvalidOperationException
                // (assume that the dates that we write, can be read back)
                DateOfBirth = (DateTime) reader.ReadDate("dob")
            };
        }

        // serialize
        public override void Write(ICompactWriter writer, Person value)
        {
            writer.WriteString("name", value.Name);
            writer.WriteString("surname", value.Surname);

            // compact serialization does not directly supports DateTime but
            // supports the HLocalDate structure which is equivalent
            writer.WriteDate("dob", (HLocalDate) value.DateOfBirth);
        }
    }
}
