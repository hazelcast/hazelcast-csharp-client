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
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Org.Website.Samples
{
    public class QuerySample
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

        public static void Run(string[] args)
        {
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var clientConfig = new ClientConfig();
            clientConfig.GetSerializationConfig()
                .AddPortableFactory(PortableFactory.FactoryId, new PortableFactory());
            var hz = HazelcastClient.NewHazelcastClient(clientConfig);
            // Get a Distributed Map called "users"
            var users = hz.GetMap<string, User>("users");
            // Add some users to the Distributed Map
            GenerateUsers(users);
            // Create a Predicate from a String (a SQL like Where clause)
            var sqlQuery = Predicates.Sql("active AND age BETWEEN 18 AND 21)");
            // Creating the same Predicate as above but with a builder
            var criteriaQuery = Predicates.And(
                Predicates.IsEqual("active", true),
                Predicates.IsBetween("age", 18, 21)
            );
            // Get result collections using the two different Predicates
            var result1 = users.Values(sqlQuery);
            var result2 = users.Values(criteriaQuery);
            // Print out the results
            Console.WriteLine(result1);
            Console.WriteLine(result2);
            // Shutdown this Hazelcast Client
            hz.Shutdown();
        }

        private static void GenerateUsers(IMap<string, User> users)
        {
            users.Put("Rod", new User("Rod", 19, true));
            users.Put("Jane", new User("Jane", 20, true));
            users.Put("Freddy", new User("Freddy", 23, true));
        }
    }
}