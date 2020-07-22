// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.DistributedObjects;
using Hazelcast.Predicates;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.WebSite
{
    // ReSharper disable once UnusedMember.Global
    public class QueryExample : ExampleBase
    {
        public class User : IPortable
        {
            public const int ClassId = 1;

            private string _username;
            private int _age;
            private bool _active;

            public User()
            {
            }

            public User(string username, int age, bool active)
            {
                _username = username;
                _age = age;
                _active = active;
            }

            public int GetFactoryId()
            {
                return PortableFactory.FactoryId;
            }

            public int GetClassId()
            {
                return ClassId;
            }

            public void ReadPortable(IPortableReader reader)
            {
                _username = reader.ReadUTF("username");
                _age = reader.ReadInt("age");
                _active = reader.ReadBoolean("active");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteUTF("username", _username);
                writer.WriteInt("age", _age);
                writer.WriteBoolean("active", _active);
            }

            public override string ToString() => $"User: {_username}, {_age}, {(_active?"":"not ")}active.";
        }

        public class PortableFactory : IPortableFactory
        {
            public const int FactoryId = 1;

            public IPortable Create(int classId)
            {
                if (classId == User.ClassId) return new User();
                return null;
            }
        }

        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            var options = BuildExampleOptions(args);
            options.Serialization.AddPortableFactory(PortableFactory.FactoryId, new PortableFactory());
            await using var client = new HazelcastClientFactory(options).CreateClient();
            await client.ConnectAsync();

            // Get a Distributed Map called "users"
            await using var users = await client.GetMapAsync<string, User>("users");
            // Add some users to the Distributed Map
            await GenerateUsers(users);
            // Create a Predicate from a String (a SQL like Where clause)
            var sqlQuery = Predicate.Sql("active AND age BETWEEN 18 AND 21)");
            // Creating the same Predicate as above but with a builder
            var criteriaQuery = Predicate.And(
                Predicate.IsEqual("active", true),
                Predicate.IsBetween("age", 18, 21)
            );
            // Get result collections using the two different Predicates
            var result1 = await users.GetValuesAsync(sqlQuery);
            var result2 = await users.GetValuesAsync(criteriaQuery);
            // Print out the results
            Console.WriteLine("Result1:");
            foreach (var result in result1) Console.WriteLine(result);
            Console.WriteLine("Result2:");
            foreach (var result in result2) Console.WriteLine(result);
        }

        private static async Task GenerateUsers(IHMap<string, User> users)
        {
            await users.AddAsync("Rod", new User("Rod", 19, true));
            await users.AddAsync("Jane", new User("Jane", 20, true));
            await users.AddAsync("Freddy", new User("Freddy", 23, true));
        }
    }
}
